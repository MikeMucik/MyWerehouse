using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Queries.GetProductCount;
using MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetVirtualPallets;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Application.Products.Queries.GetNumberPalletsAndRest;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.UpdateIssue
{
	public class UpdateIssueNewHandler : IRequestHandler<UpdateIssueNewCommand, List<IssueResult>>
	{
		private readonly IIssueItemRepo _issueItemRepo;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IEventCollector _eventCollector;
		private readonly IProductRepo _productRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		public UpdateIssueNewHandler(IIssueItemRepo issueItemRepo
			, IIssueRepo issueRepo,
				IMediator mediator,
				WerehouseDbContext werehouseDbContext,
				IEventCollector eventCollector,
				IProductRepo productRepo,IAddPickingTaskToIssueService addPickingTaskToIssueService)
		{
			_issueItemRepo = issueItemRepo;
			_issueRepo = issueRepo;
			_mediator = mediator;
			_werehouseDbContext = werehouseDbContext;
			_eventCollector = eventCollector;
			_productRepo = productRepo;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
		}
		public async Task<List<IssueResult>> Handle(UpdateIssueNewCommand request, CancellationToken ct)
		{
			var resultList = new List<IssueResult>();
			var issue = await _issueRepo.GetIssueByIdAsync(request.DTO.Id) ?? throw new NotFoundIssueException(request.DTO.Id);
			// Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone lub nie zaczęty picking
			if (issue.IssueStatus == IssueStatus.New ||
				issue.IssueStatus == IssueStatus.Pending ||
				issue.IssueStatus == IssueStatus.NotComplete)
			{
				await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
				//Kasowanie starych
				try
				{
					//if (await _productRepo.IsExistProduct(request.Product.ProductId) == false) throw new ProductException($"Brak produktu o numerze{request.Product.ProductId} .");
					var oldListPallets = new List<Pallet>();
					var listOldPallets = issue.Pallets.ToList();
					foreach (var pallet in listOldPallets)
					{
						issue.Pallets.Remove(pallet);
						pallet.Status = PalletStatus.InTransit;
						oldListPallets.Add(pallet);
					}
					var listOldPickingTask = issue.PickingTasks.ToList();
					foreach (var pickingTask in listOldPickingTask)
					{
						var historyPickingTaskOld = new CreateHistoryPickingNotification(new HistoryDataPicking(
							pickingTask.Id,
							pickingTask.VirtualPallet.PalletId,
							pickingTask.IssueId,
							pickingTask.ProductId,
							pickingTask.Quantity,
							0,
							pickingTask.PickingStatus,
							PickingStatus.Cancelled,
							request.DTO.PerformedBy,
							DateTime.UtcNow));
						await _mediator.Publish(historyPickingTaskOld, ct);
						issue.PickingTasks.Remove(pickingTask);
						_werehouseDbContext.PickingTasks.Remove(pickingTask);
					}
					await _werehouseDbContext.SaveChangesAsync(ct);
					var palletAssigned = new List<Pallet>();
					var anyFailure = false; // Flaga, czy wystąpił jakikolwiek błąd
					var anySuccess = false; // Flaga, czy cokolwiek się udało
					foreach (var product in request.DTO.Items) //bo dla każdego osobno i memo na koniec
					{
						await transaction.CreateSavepointAsync($"BeforeProduct_{product.ProductId}", ct);
						try
						{
							if (await _productRepo.IsExistProduct(product.ProductId) == false) throw new NotFoundProductException(product.ProductId);
							var existingIssueItem = issue.IssueItems
								.FirstOrDefault(x => x.ProductId == product.ProductId);
							var oldProperPallets = oldListPallets.Where(p => p.ProductsOnPallet.First().ProductId == product.ProductId);
							//1 dostępność towaru							
							var totalAvailable = await _mediator.Send(new GetProductCountQuery(product.ProductId, product.BestBefore), ct);
							if (product.Quantity > totalAvailable)//
							{
								resultList.Add(IssueResult.Fail($"Nie wystarczająca ilości produktu o numerze {product.ProductId}. Asortyment nie został dodany do zlecenia."
									, product.ProductId));
								anyFailure = true;
								continue;
							}
							//2 Oblicz pełne palety i resztę 
							var palletAmountFullResult = await _mediator.Send(new GetNumberPalletsAndRestQuery(product.ProductId, product.Quantity), ct);
							if (palletAmountFullResult.Success is false)
							{
								resultList.Add(IssueResult.Fail(palletAmountFullResult.Message));
								anyFailure = true;
								continue;
							}
							var amountPallets = palletAmountFullResult.FullPallet;
							var rest = palletAmountFullResult.Rest;
							//3. Pobierz dostępne palety 
							var availablePallets = await _mediator.Send(new GetAvailablePalletsByProductQuery(product.ProductId, product.BestBefore, amountPallets + 1, product.Quantity), ct);
							var allAvailablePallets = oldProperPallets
								.Concat(availablePallets)
								.DistinctBy(p => p.Id)
								.ToList();
							//3.1 pobierz dostępne virtualPallet
							var availableVirtualPalletsQuery = await _mediator.Send(new GetVirtualPalletsQuery(product.ProductId, product.BestBefore), ct);
							if (availableVirtualPalletsQuery is null)
							{
								resultList.Add(IssueResult.Fail("Brak palety do pickingu - błąd virtual"));
								anyFailure = true;
								continue;
							}
							//4. Przydziel pełne palety
							palletAssigned = await _mediator.Send(new AssignFullPalletToIssueCommand(issue, allAvailablePallets, amountPallets), ct);
							//5. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
							if (rest > 0)
							{
								var newPickingTaskFromRest = await _addPickingTaskToIssueService.AddPickingTaskToIssue(
									palletAssigned, availableVirtualPalletsQuery, issue,
									product.ProductId, rest, product.BestBefore, request.DTO.PerformedBy);
								if (newPickingTaskFromRest.Success is false)
								{
									resultList.Add(IssueResult.Fail(newPickingTaskFromRest.Message));
									anyFailure = true;
									continue;
								}
							}
							var returnPallets = oldProperPallets.Except(palletAssigned).ToList();
							foreach (var returnPallet in returnPallets)
							{
								returnPallet.IssueId = null;
								returnPallet.Status = PalletStatus.Available;
							}
							await _werehouseDbContext.SaveChangesAsync(ct);
							foreach (var evn in _eventCollector.Events)
							{
								await _mediator.Publish(evn, CancellationToken.None);
							}
							foreach (var factory in _eventCollector.DeferredEvents)
							{
								await _mediator.Publish( factory(), ct);
							}
							resultList.Add(IssueResult.Ok("Towar dołączono do wydania", product.ProductId));
							anySuccess = true;

						}
						catch (Exception ex) // Łapiemy wszystko tutaj, żeby obsłużyć logikę czyszczenia
						{
							await transaction.RollbackToSavepointAsync($"BeforeProduct_{product.ProductId}", ct);
							await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
							await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
							await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);
							//await transaction.RollbackAsync(ct);
							anyFailure = true;
							// Obsługa konkretnych wyjątków do wyniku
							if (ex is NotFoundProductException pEx)
							{
								resultList.Add(IssueResult.Fail(pEx.Message, product.ProductId));
							}
							else if (ex is NotFoundPalletException palEx)
							{
								resultList.Add(IssueResult.Fail(palEx.Message, product.ProductId));
							}
							else if (ex is NotFoundIssueException ei)
							{
								resultList.Add(IssueResult.Fail(ei.Message, product.ProductId));
							}
							else if (ex is DbUpdateConcurrencyException)
							{
								resultList.Add(IssueResult.Fail("Inny użytkownik operuje ..."));
							}
							else
							{
								// Logowanie krytyczne
								// _logger.LogError(ex, ...)
								resultList.Add(IssueResult.Fail("Wystąpił nieoczekiwany błąd", product.ProductId));
								// Tutaj DECYZJA: Czy throw? Jeśli rzucisz throw, przerwiesz pętlę dla kolejnych produktów.
								// Jeśli chcesz kontynuować dla innych produktów, nie rób throw.
							}
						}
						finally
						{
							_eventCollector.Clear();
						}
					}
					var touchedVirtualPalletIds = listOldPickingTask
						.Select(a => a.VirtualPalletId)
						.Distinct()
						.ToList();

					// Pobieramy kandydatów do usunięcia WRAZ z ich fizycznymi paletami
					var virtualPalletsToCheck = await _werehouseDbContext.VirtualPallets
						.Include(vp => vp.Pallet) // Ważne!
						.Where(vp => touchedVirtualPalletIds.Contains(vp.Id))
						.ToListAsync(ct);

					var emptyVirtualPallets = virtualPalletsToCheck
						.Where(vp => vp.PickingTasks.Count == 0) // Sprawdzamy w pamięci (EF powinien widzieć zmiany z pętli)
						.ToList();

					foreach (var vp in emptyVirtualPallets)
					{
						if (vp.Pallet != null)
						{
							vp.Pallet.Status = PalletStatus.Available;
						}
						_werehouseDbContext.VirtualPallets.Remove(vp);
					}
					if (anySuccess)
					{
						issue.PerformedBy = request.DTO.PerformedBy;
						issue.IssueStatus = IssueStatus.Pending;
					}
					if (anyFailure)
					{
						issue.IssueStatus = IssueStatus.NotComplete;
					}
					foreach (var palletToStock in oldListPallets)
					{
						if (!issue.Pallets.Any(p => p.Id == palletToStock.Id))
						{
							if (palletToStock.IssueId == null || palletToStock.IssueId == issue.Id)
							{
								palletToStock.Status = PalletStatus.Available;
								palletToStock.IssueId = null;
							}
						}
					}
					await _werehouseDbContext.SaveChangesAsync(ct);
					await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, request.DTO.PerformedBy), ct);//					
					await transaction.CommitAsync(ct);
					return resultList;
				}
				catch (Exception fatalEx)
				{
					// Jeśli padnie coś krytycznego poza pętlą -> Rollback wszystkiego
					await transaction.RollbackAsync(ct);
					throw;
				}
			}
			else if (issue.IssueStatus == IssueStatus.ConfirmedToLoad)
			{
				var newQuantities = new List<IssueItemDTO>();
				var hasNegativeDiff = false;
				var errorMessage = new List<string>();
				foreach (var product in request.DTO.Items)
				{
					var productId = product.ProductId;
					var oldQuantity = await _issueItemRepo.GetQuantityByIssueAndProduct(issue, productId);
					var newQuantity = product.Quantity - oldQuantity;
					if (newQuantity < 0)
					{
						hasNegativeDiff = true;
						errorMessage.Add($"Produkt {productId}: Nie można zmniejszyć z {oldQuantity} do {product.Quantity} (różnica : {newQuantity}). Zlecenie jest już zatwierdzone do załadunku");
						continue;
					}
					if (newQuantity > 0)
					{
						var newItem = new IssueItemDTO
						{
							ProductId = productId,
							Quantity = newQuantity,
							BestBefore = product.BestBefore
						};
						newQuantities.Add(newItem);
					}
				}
				if (hasNegativeDiff)
				{
					return new List<IssueResult>
					{
						IssueResult.Fail(string.Join(";", errorMessage),0)
					};
				}
				if (newQuantities.Count == 0)
				{
					return new List<IssueResult> { IssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.", 0) };
				}
				var dataForNewIssue = new CreateIssueDTO
				{
					ClientId = issue.ClientId,
					Items = newQuantities,
					PerformedBy = request.DTO.PerformedBy,
				};
				resultList = await _mediator.Send(new CreateNewIssueCommand(dataForNewIssue, request.DateToSend), ct);
				foreach (var result in resultList)
				{
					if (result.Success)
					{
						result.Message += " (Dodatkowe zlecenie na ostatnią chwilę - stare jest w realizacji).";
					}
				}
				return resultList;
			}
			else
			{
				resultList.Add(IssueResult.Fail(
					$"Nie można zaktualizować zlecenia {issue.Id}, status: {issue.IssueStatus}"));
				return resultList;
			}
		}

	}
}
