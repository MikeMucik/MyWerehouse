using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Inventories.Queries.GetProductCount;
using MyWerehouse.Application.Issues.Commands.AddPalletsToIssueByProduct;
using MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct;
using MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue;
using MyWerehouse.Application.PickingPallets.Qeuries.GetVirtualPallets;
using MyWerehouse.Application.Products.Queries.GetNumberUnitOnPallet;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Issues.Commands.UpdateIssue
{
	public class UpdateIssueHandler : IRequestHandler<UpdateIssueCommand, List<IssueResult>>
	{
		private readonly IIssueItemRepo _issueItemRepo;
		private readonly IIssueRepo _issueRepo;
		//private readonly IPalletRepo _palletRepo;
		//private readonly IAllocationRepo _allocationRepo;
		//private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IMediator _mediator;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IEventCollector _eventCollector;
		public UpdateIssueHandler(IIssueItemRepo issueItemRepo
			, IIssueRepo issueRepo,
				//IPalletRepo palletRepo,
				//IAllocationRepo allocationRepo,
				//IPickingPalletRepo pickingPalletRepo,
				IMediator mediator,
				WerehouseDbContext werehouseDbContext,
				IEventCollector eventCollector)
		{
			_issueItemRepo = issueItemRepo;
			_issueRepo = issueRepo;
			//_palletRepo = palletRepo;
			//_allocationRepo = allocationRepo;
			//_pickingPalletRepo = pickingPalletRepo;
			_mediator = mediator;
			_werehouseDbContext = werehouseDbContext;
			_eventCollector = eventCollector;
		}
		public async Task<List<IssueResult>> Handle(UpdateIssueCommand request, CancellationToken ct)
		{
			var resultList = new List<IssueResult>();
			var issue = await _issueRepo.GetIssueByIdAsync(request.DTO.Id) ?? throw new IssueException(request.DTO.Id);
			issue.PerformedBy = request.DTO.PerformedBy;             // Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone lub nie zaczęty picking
			if (issue.IssueStatus == IssueStatus.New ||
				issue.IssueStatus == IssueStatus.Pending ||
				issue.IssueStatus == IssueStatus.NotComplete)
			{
				{
					//var issueItemsAdded = new List<IssueItem>();
					var oldListPallets = issue.Pallets;
					var oldListAllocations = issue.Allocations;
					//chyba muszę mieć synchronizer: palet, alokacji(co z virtualPallet)
					var listOfAllocation = new List<Allocation>();
					var palletAssigned = new List<Pallet>();
					foreach (var product in request.DTO.Items) //bo dla każdego osobno i memo na koniec
					{
						await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
						var totalAvailable = 0;
						try
						{
							var existingIssueItem = issue.IssueItems
								.FirstOrDefault(x => x.ProductId == product.ProductId);
							var newAmountOfProduct = product.Quantity;
							var oldAmountOfProduct = existingIssueItem?.Quantity ?? 0;
							if (newAmountOfProduct < oldAmountOfProduct) throw new ProductException($"Nowa ilość produktu {product.ProductId} nie może być mniejsza od starej");
							var addedNewCartoonsForProduct = newAmountOfProduct - oldAmountOfProduct;
							if (addedNewCartoonsForProduct == 0) continue;
							//1 dostępność towaru
							totalAvailable = await _mediator.Send(new GetProductCountQuery(product.ProductId, product.BestBefore), ct);
							if (product.Quantity > totalAvailable)//
							{
								throw new ProductException($"Nie wystarczająca ilości produktu o numerze {product.ProductId}. Asortyment nie został dodany do zlecenia.");
							}
							//2 Oblicz pełne palety i resztę - to można wyodrębnić
							var palletAmountFullResult = await _mediator.Send(new GetNumberPalletsAndRestQuery(product.ProductId, addedNewCartoonsForProduct), ct);
							var amountPallets = palletAmountFullResult.FullPallet;
							var rest = palletAmountFullResult.Rest;
							//3. Pobierz dostępne palety - tu trzeba dodać blokadę 
							var availablePallets = await _mediator.Send(new GetAvailablePalletsByProductQuery(product.ProductId, product.BestBefore, amountPallets + 1, addedNewCartoonsForProduct), ct);
							//3.1 pobierz dostępne virtualPallet
							var availableVirtualPalletsQuery = await _mediator.Send(new GetVirtualPalletsQuery(product.ProductId, product.BestBefore), ct);
							//4. Przydziel pełne palety
							palletAssigned = await _mediator.Send(new AssignFullPalletToIssueCommand(issue, availablePallets, amountPallets), ct);
							var restPallet = availablePallets.Except(palletAssigned).ToList();
							//5. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
							if (rest > 0)
							{
								var newAllocationFromRest = await _mediator.Send(
									new AddAllocationToIssueCommand(restPallet, availableVirtualPalletsQuery, issue,
									product.ProductId,
									rest, product.BestBefore,
									request.DTO.PerformedBy
									), ct);
							}
							await _werehouseDbContext.SaveChangesAsync(ct);
							issue.IssueStatus = IssueStatus.ChangingPallet;
							foreach (var evn in _eventCollector.Events)
							{
								await _mediator.Publish(evn, CancellationToken.None);
							}
							foreach (var factory in _eventCollector.DeferredEvents)
							{
								await _mediator.Publish(await factory(), ct);
							}
							await transaction.CommitAsync(ct);

							_eventCollector.Clear();

							resultList.Add(IssueResult.Ok("Towar dołączono do wydania", product.ProductId));

							await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, issue.PerformedBy), ct);//
						}
						catch (ProductException expr)//
						{
							await transaction.RollbackAsync(ct);
							resultList.Add(
							IssueResult.Fail(
							   expr.Message,
							   product.ProductId,
							   product.Quantity,
							   totalAvailable));
						}
						catch (PalletException expal)
						{
							await transaction.RollbackAsync(ct);
							resultList.Add(IssueResult.Fail(
								expal.Message,
								product.ProductId));
						}
						catch (IssueException ei)
						{
							await transaction.RollbackAsync(ct);
							resultList.Add(IssueResult.Fail(
								ei.Message, product.ProductId));
						}
						catch (DbUpdateConcurrencyException)
						{
							await transaction.RollbackAsync(ct);
							resultList.Add(IssueResult.Fail("Inny użytkownik operuje ..."));
						}
						catch (Exception ex)
						{
							await transaction.RollbackAsync(ct);
							// Loguj ex dla developera!
							//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
							throw new InvalidOperationException("Wystąpił błąd podczas przypisywania palet do zlecenia.", ex.InnerException);
						}
					}
					var freshPallets = oldListPallets.Concat(palletAssigned).ToList();
					CollectionSynchronizer.SynchronizeCollection(
						oldListPallets,
						freshPallets,
						x => x.Id,
						x => x.Id,
						addMapper: p => p,
						updateMapper: (src, dst) => { },
						removeMapper: p => { p.IssueId = null; p.Status = PalletStatus.Available; });
				}			
				return resultList;			
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