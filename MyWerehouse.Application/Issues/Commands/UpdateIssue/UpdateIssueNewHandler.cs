using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssuesServices;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.UpdateIssue
{
	public class UpdateIssueNewHandler(IIssueItemRepo issueItemRepo,
		IIssueRepo issueRepo,
		IMediator mediator,
		WerehouseDbContext werehouseDbContext,
		IAssignProductToIssueService assignProductToIssueAsync) : IRequestHandler<UpdateIssueNewCommand, AppResult<List<IssueResult>>>
	{
		private readonly IIssueItemRepo _issueItemRepo = issueItemRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMediator _mediator = mediator;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IAssignProductToIssueService _assignProductToIssueAsync = assignProductToIssueAsync;
		public async Task<AppResult<List<IssueResult>>> Handle(UpdateIssueNewCommand request, CancellationToken ct)
		{
			var resultList = new List<IssueResult>();
			var issue = await _issueRepo.GetIssueByIdAsync(request.DTO.Id);
			if (issue == null)
				return AppResult<List<IssueResult>>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);

			// Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone lub nie zaczęty picking
			if (issue.IssueStatus == IssueStatus.New ||
				issue.IssueStatus == IssueStatus.Pending ||
				issue.IssueStatus == IssueStatus.NotComplete)
			{
				await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
				//Kasowanie starych
				try
				{
					var oldListPallets = new List<Pallet>();//
					var listOldPallets = issue.Pallets.ToList();//
					foreach (var pallet in listOldPallets)
					{
						issue.Pallets.Remove(pallet);
						pallet.Status = PalletStatus.InTransit;
						oldListPallets.Add(pallet);
					}
					var listOldPickingTask = issue.PickingTasks.ToList();
					//remove old PickingTask to Domain
					foreach (var pickingTask in listOldPickingTask)
					{
						pickingTask.AddHistory(request.DTO.PerformedBy, pickingTask.PickingStatus, PickingStatus.Cancelled, 0);
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
							var oldProperPallets = oldListPallets.Where(p => p.ProductsOnPallet.First().ProductId == product.ProductId);

							var result = await _assignProductToIssueAsync.AssignProductToIssue(issue, product, IssueAllocationPolicy.FullPalletFirst, oldListPallets, request.DTO.PerformedBy);
							//if (result.Success == false)
							//{
							//	resultList.Add(IssueResult.Fail(result.Message, product.ProductId));

							if (!result.Success)
							{
								await transaction.RollbackToSavepointAsync($"BeforeProduct_{product.ProductId}", ct);
								await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
								await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
								await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);

								resultList.Add(IssueResult.Fail(result.Message, product.ProductId));
								anyFailure = true;
								continue;
							}

							//anyFailure = true;
							//continue;
						//}
							palletAssigned = result.AssignedPallets.ToList();
							var returnPallets = oldProperPallets.Except(palletAssigned).ToList();
							foreach (var returnPallet in returnPallets)
							{
								returnPallet.IssueId = null;
								returnPallet.Status = PalletStatus.Available;
							}
							await _werehouseDbContext.SaveChangesAsync(ct);
							resultList.Add(IssueResult.Ok("Towar dołączono do wydania", product.ProductId));
							anySuccess = true;

						}
						catch (Exception ex) // Łapiemy wszystko tutaj, żeby obsłużyć logikę czyszczenia
						{
							//await transaction.RollbackToSavepointAsync($"BeforeProduct_{product.ProductId}", ct);
							//await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
							//await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
							//await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);
							//anyFailure = true;
							//// Obsługa konkretnych wyjątków do wyniku
							//if (ex is NotFoundProductException pEx)
							//{
							//	resultList.Add(IssueResult.Fail(pEx.Message, product.ProductId));
							//}
							//else if (ex is NotFoundPalletException palEx)
							//{
							//	resultList.Add(IssueResult.Fail(palEx.Message, product.ProductId));
							//}
							//else if (ex is NotFoundIssueException ei)
							//{
							//	resultList.Add(IssueResult.Fail(ei.Message, product.ProductId));
							//}
							//else if (ex is DbUpdateConcurrencyException)
							//{
							//	resultList.Add(IssueResult.Fail("Inny użytkownik operuje ..."));
							//}
							//else
							//{
								// Logowanie krytyczne
								// _logger.LogError(ex, ...)
								resultList.Add(IssueResult.Fail("Wystąpił nieoczekiwany błąd", product.ProductId));
								// Tutaj DECYZJA: Czy throw? Jeśli rzucisz throw, przerwiesz pętlę dla kolejnych produktów.
								// Jeśli chcesz kontynuować dla innych produktów, nie rób throw.
							//}
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
					issue.AddHistory(request.DTO.PerformedBy);
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					//return resultList;
					return AppResult<List<IssueResult>>.Success(resultList);
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
					return AppResult<List<IssueResult>>.Fail(
						  string.Join(";", errorMessage),
					   ErrorType.Conflict // lub Validation, zależnie od semantyki
				   );
				}
				//return new AppResult<List<IssueResult>>
				//{
				//	IssueResult.Fail(string.Join(";", errorMessage),0)
				//};

				if (newQuantities.Count == 0)
				{
					var resultListNoQuantitesChange = new List<IssueResult>
					{
						IssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.", 0)
					};
					return AppResult<List<IssueResult>>.Success(resultListNoQuantitesChange);
				}
				//return AppResult<List<IssueResult>>.Success(List<IssueResult>., "Brak zmian w ilościach - zlecenie bez modyfikacji.");
					//return new List<IssueResult> { IssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.", 0) };
				//}
				var dataForNewIssue = new CreateIssueDTO
				{
					ClientId = issue.ClientId,
					Items = newQuantities,
					PerformedBy = request.DTO.PerformedBy,
				};
				var receiverFromCreate =  await _mediator.Send(new CreateNewIssueCommand(dataForNewIssue, request.DateToSend), ct);
				if (receiverFromCreate.IsSuccess is false)
					return AppResult<List<IssueResult>>.Fail("Nie udało się utworzyć nowego zlecenia.", ErrorType.Conflict); //Technical??
				resultList = receiverFromCreate.Result.ToList();
				foreach (var result in resultList)
				{
					if (result.Success)
					{
						result.Message += " (Dodatkowe zlecenie na ostatnią chwilę - stare jest w realizacji).";
					}
				}
				//return resultList;
				return AppResult<List<IssueResult>>.Success(resultList);
			}
			else
			{
				resultList.Add(IssueResult.Fail(
					$"Nie można zaktualizować zlecenia {issue.Id}, status: {issue.IssueStatus}"));
				return AppResult<List<IssueResult>>.Success(resultList);
				//return resultList;
			}
		}

	}
}
