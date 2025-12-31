using AutoMapper;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet;
using MyWerehouse.Application.PickingPallets.Commands.CreatePalletOrAddToPallet;
using MyWerehouse.Application.PickingPallets.Commands.DoPicking;
using MyWerehouse.Application.PickingPallets.Commands.ExecuteManualPicking;
using MyWerehouse.Application.PickingPallets.Commands.PrepareManualPicking;
using MyWerehouse.Application.PickingPallets.Commands.ProcessPickingAction;
using MyWerehouse.Application.PickingPallets.Commands.ReduceAllocation;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet;
using MyWerehouse.Application.PickingPallets.Queries.GetListToPicking;
using MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Services
{
	public class PickingPalletService : IPickingPalletService
	{
		private readonly IMediator _mediator;		
		public PickingPalletService(
			IMediator mediator)			
		{
			_mediator = mediator;			
		}

		//Part to read
		//lista palet do zdjęcia przez wózkowego pallet's list for operator
		public async Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd)
		//lista palet dla wózkowego do dostaczenia do strafy pickingu
		{
			return await _mediator.Send(new GetListPickingPalletQuery(dateMovedStart, dateMovedEnd));
			//var pickingPallets = new List<PickingPalletWithLocationDTO>();
			//var palletPicking = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateMovedStart, dateMovedEnd);
			//foreach (var pallet in palletPicking)
			//{
			//	var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
			//	if (locationName == null) throw new InvalidDataException($"Brak lokalizacji {pallet.LocationId} w magazynie");//It's shouldn't heppend
			//																												  //var addedToPicking = await _pickingPalletRepo.TakeDateAddedToPickingAsync(pallet.Id);
			//	var addedToPicking = pallet.DateMoved;
			//	var palletInWarehouseDTO = new PickingPalletWithLocationDTO
			//	{
			//		PalletId = pallet.PalletId,
			//		LocationName = locationName.Bay + " " + locationName.Aisle + " " + locationName.Position + " " + locationName.Height,
			//		AddedToPicking = addedToPicking
			//	};
			//	pickingPallets.Add(palletInWarehouseDTO);
			//}
			//return pickingPallets;
		}
		//Lista ile danego towaru dla danego zlecenia posegregowane i zgrupowane po kliencie Product's list by issue&client
		public async Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd)
		{
			return await _mediator.Send(new GetListIssueToPickingQuery(dateIssueStart, dateIssueEnd));
			//var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			//if (pickingPallets.Count == 0)
			//{
			//	return new List<PickingGuideLineDTO>();
			//}
			//var allNededIssuesIds = pickingPallets
			//	.SelectMany(p => p.Allocations)
			//	.Select(i => i.IssueId)
			//	.Distinct()
			//	.ToList();

			//var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);
			//var issueDictionary = allIssues.ToDictionary(i => i.Id);
			//return [.. pickingPallets
			//	.SelectMany(p => p.Allocations.Select(a => new
			//	{
			//		IssueId = a.IssueId,
			//		Quantity = a.Quantity,
			//		ProductId = p.Pallet.ProductsOnPallet.First().ProductId,
			//		ClientIdOut = issueDictionary[a.IssueId].ClientId
			//	}))
			//	.GroupBy(x => x.ClientIdOut)
			//	.Select(clientGroup => new PickingGuideLineDTO
			//	{
			//		ClientIdOut = clientGroup.Key,
			//		Issues = [.. clientGroup
			//			.GroupBy(a => a.IssueId)
			//			.Select(issueGroup => new IssueForPickingDTO
			//			{
			//				IssueId = issueGroup.Key,
			//				Products = [.. issueGroup
			//					.GroupBy(a => a.ProductId)
			//					.Select(prodGroup => new ProductOnPalletPickingDTO
			//					{
			//						ProductId = prodGroup.Key,
			//						Quantity = prodGroup.Sum(x => x.Quantity)
			//					})
			//					.OrderBy(p => p.ProductId)]
			//			})
			//			.OrderBy(i => i.IssueId)]
			//	})
			//	.OrderBy(c => c.ClientIdOut)];
		}
		//Lista ile danego towaru dla danej alokacji Product's list by allocations
		public async Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd)
		//wytyczne- lista ile jakiego produktu do konkretnego zlecenia - zlecenia na daną chwilę, bierzemy zlecenia z danego okresu/dnia
		// pojedyncze rekordy dla każdej alokacji
		{
			return await _mediator.Send(new GetListToPickingQuery(dateIssueStart, dateIssueEnd));
			//var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			//if (pickingPallets.Count == 0)
			//{
			//	return new List<ProductToIssueDTO>();
			//}
			//var allNededIssuesIds = pickingPallets
			//	.SelectMany(p => p.Allocations)
			//	.Select(i => i.IssueId)
			//	.Distinct()
			//	.ToList();

			//var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);

			//var issueDictionary = allIssues.ToDictionary(i => i.Id);

			//var aggregationDictionary = new Dictionary<(int ClientId, int
			//	IssueId, int product), ProductToIssueDTO>();

			//foreach (var pallet in pickingPallets)
			//{
			//	var productOnPallet = pallet.Pallet?.ProductsOnPallet?.FirstOrDefault();
			//	if (productOnPallet == null) continue;
			//	var productId = productOnPallet.ProductId;

			//	var allocations = pallet.Allocations;
			//	foreach (var allocation in allocations)
			//	{
			//		if (!issueDictionary.TryGetValue(allocation.IssueId, out var issue))
			//		{
			//			continue;
			//		}
			//		var clientId = issue.ClientId;
			//		var key = (clientId, allocation.IssueId, productId);
			//		if (aggregationDictionary.TryGetValue(key, out var existingRecord))
			//		{
			//			existingRecord.Quantity += allocation.Quantity;
			//		}
			//		else
			//		{
			//			var productIssue = new ProductToIssueDTO
			//			{
			//				ClientIdOut = clientId,
			//				IssueId = allocation.IssueId,
			//				ProductId = productId,
			//				Quantity = allocation.Quantity,
			//			};
			//			aggregationDictionary.Add(key, productIssue);
			//		}
			//	}
			//}
			//return aggregationDictionary
			//		.OrderBy(x => x.Key.ClientId)
			//			.ThenBy(x => x.Key.IssueId)
			//				.ThenBy(x => x.Key.product)
			//		.Select(x => x.Value)
			//		.ToList();
		}

		//Part to write&read
		//pokaż alokacje dla palety Show allocations to do - scan pallet
		public async Task<List<AllocationDTO>> ShowTaskToDoAsync(string palletSourceScanned, DateTime pickingDate)
		{
			return await _mediator.Send(new ShowTaskToDoQuery(palletSourceScanned, pickingDate));
			//var palletPickingId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletSourceScanned);
			//var allocations = await _allocationRepo.GetAllocationListAsync(palletPickingId, pickingDate);
			////mapper??
			//return allocations.Select(allocation => new AllocationDTO
			//{
			//	AllocationId = allocation.Id,
			//	IssueId = allocation.IssueId,
			//	SourcePalletId = allocation.VirtualPallet.Pallet.Id,
			//	ProductId = allocation.VirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
			//	PickingStatus = allocation.PickingStatus,
			//	RequestedQuantity = allocation.Quantity,
			//	BestBefore = allocation.VirtualPallet.Pallet.ProductsOnPallet.First().BestBefore
			//}).ToList();
		}
		//faktyczne działanie pickingu - zmiany w bazie Do pick - arranging goods
		public async Task<PickingResult> DoPickingAsync(AllocationDTO allocationDTO, string userId)
		{
			return await _mediator.Send(new DoPickingCommand(allocationDTO, userId));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();			
			//try
			//{
			//	var newVirtualPallet = new VirtualPallet();
			//	var newAllocation = new Allocation();
			//	var allocationToChange = await _allocationRepo.GetAllocationAsync(allocationDTO.AllocationId);
			//	var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(allocationToChange.VirtualPalletId);
			//	var issueId = allocationToChange.IssueId;
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueException(issueId);
			//	var sourcePallet = await _palletRepo.GetPalletByIdAsync(allocationDTO.SourcePalletId)
			//		?? throw new PalletException(allocationDTO.SourcePalletId);
			//	await ProcessPickingActionAsync(sourcePallet, issue, allocationDTO.ProductId, allocationDTO.PickedQuantity, userId);
			//	if (allocationDTO.RequestedQuantity == allocationDTO.PickedQuantity)
			//	{
			//		allocationToChange.PickingStatus = PickingStatus.Picked;
			//		var historyPicking = new HistoryDataPicking
			//				(
			//					allocationToChange.Id,
			//					allocationToChange.VirtualPallet.PalletId,
			//					allocationToChange.IssueId,
			//						 allocationToChange.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 allocationToChange.Quantity,
			//						 0,
			//						 PickingStatus.Allocated,
			//						 allocationToChange.PickingStatus,
			//						 userId,
			//						 DateTime.UtcNow
			//					);
			//		_eventCollector.Add(new CreateHistoryPickingNotification(
			//				historyPicking
			//				));
			//	}
			//	else if (allocationDTO.RequestedQuantity > allocationDTO.PickedQuantity)
			//	{
			//		var newQuantityToAllocation = allocationDTO.RequestedQuantity - allocationDTO.PickedQuantity;
					
			//		newVirtualPallet = 	await _mediator.Send(new AddPalletToPickingCommand(issue, allocationDTO.ProductId, allocationDTO.BestBefore, userId, [] ));
			//		newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, newQuantityToAllocation);
			//		_allocationRepo.AddAllocation(newAllocation);
			//		var historyPicking = new HistoryDataPicking
			//				(
			//					newAllocation.Id,
			//					newAllocation.VirtualPallet.PalletId,
			//					newAllocation.IssueId,
			//						 newAllocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 newAllocation.Quantity,
			//						 0,
			//						 PickingStatus.Allocated,
			//						 newAllocation.PickingStatus,
			//						userId,
			//						 DateTime.UtcNow
			//					);
			//		_eventCollector.Add(new CreateHistoryPickingNotification(
			//			historyPicking ));
						
			//		//zablokowanie palety źródłowej bo się nie zgadza stan fizyczny/system
			//		sourcePallet.Status = PalletStatus.OnHold;					
			//		_eventCollector.Add(new CreatePalletOperationNotification(sourcePallet.Id,
			//			sourcePallet.LocationId,
			//			ReasonMovement.Correction,
			//			userId,
			//			PalletStatus.OnHold,
			//			null));					
			//	}
				
			//	if (issue.IssueStatus == IssueStatus.Pending) { issue.IssueStatus = IssueStatus.InProgress; }
			//	//czy to ma sens

			//	if (allocationDTO.RequestedQuantity == allocationDTO.PickedQuantity)
			//	{
			//		var historyPicking = new HistoryDataPicking
			//				(
			//					allocationToChange.Id,
			//					allocationToChange.VirtualPallet.PalletId,
			//					allocationToChange.IssueId,
			//						 allocationToChange.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 allocationToChange.Quantity,
			//						 allocationToChange.Quantity,
			//						 PickingStatus.Allocated,
			//						 allocationToChange.PickingStatus,
			//						 userId,
			//						 DateTime.UtcNow
			//					);
			//		await _mediator.Publish(new CreateHistoryPickingNotification(
			//				historyPicking
			//			));
			//	}

			//	await _werehouseDbContext.SaveChangesAsync();
			//	foreach (var evn in _eventCollector.Events)
			//	{
			//		await _mediator.Publish(evn, CancellationToken.None);
			//	}
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	_eventCollector.Clear();

			//	return PickingResult.Ok("Towar dołączono do zlecenia");
			//}
			//catch (PalletException pnfEx)
			//{
			//	await transaction.RollbackAsync();
			//	return PickingResult.Fail(pnfEx.Message);
			//}
			//catch (IssueException onfEx)
			//{
			//	await transaction.RollbackAsync();
			//	return PickingResult.Fail(onfEx.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
			//	return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			//}
		}
		public async Task<PickingResult> PrepareManualPickingAsync(string palletId)
		{
			return await _mediator.Send(new PrepareManualPickingCommand(palletId));
			//var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			////Nie wyjątek bo to częsta sytuacja w rzeczywistości
			//if (pallet == null || pallet.Status == PalletStatus.Archived)
			//{
			//	return PickingResult.Fail($"Brak palety {palletId} na stanie.");
			//}

			//if (pallet.Status != PalletStatus.ToPicking)
			//{
			//	return PickingResult.Fail($"Paleta {palletId} nie jest w pickingu, zmień status.");
			//}

			//var product = pallet.ProductsOnPallet.FirstOrDefault();
			//if (product == null)
			//{
			//	return PickingResult.Fail($"Paleta {palletId} jest pusta.");
			//}
			//// Logika wyszukiwania pasujących zleceń			
			//var timeFrom = DateTime.UtcNow.AddDays(-1);
			//var timeTo = DateTime.UtcNow;
			//var allocations = await _allocationRepo.GetAllocationsProductIdAsync(product.ProductId, timeFrom, timeTo);
			//var grouped = allocations
			//	.GroupBy(a => a.IssueId)
			//	.Select(g => new IssueOptions
			//	{
			//		IssueId = g.Key,
			//		QunatityToDo = g.Sum(a => a.Quantity)
			//	})
			//	.ToList();
			//return PickingResult.RequiresOrder(
			//	productInfo: $"{product.PalletId} : {product.Quantity}",
			//	issueOptions: grouped,
			//	message: "Podaj numer zamówienia by kontynuować");
		}
		public async Task<PickingResult> ExecuteManualPickingAsync(string palletId, int issueId, string userId)
		{
			return await _mediator.Send(new ExecuteManualPickingCommand(palletId, issueId, userId));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var pallet = await _palletRepo.GetPalletByIdAsync(palletId)
			//		?? throw new PalletException(palletId);
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId)
			//		?? throw new IssueException(issueId);
			//	var product = pallet.ProductsOnPallet.FirstOrDefault()
			//		?? throw new InvalidOperationException($"Paleta {palletId} jest pusta.");

			//	// Oblicz, ile faktycznie można/trzeba skompletować
			//	var allocationsForIssue = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issueId, product.ProductId);
			//	var neededQuantity = allocationsForIssue.Where(a => a.PickingStatus == PickingStatus.Allocated).Sum(a => a.Quantity);
			//	var quantityToPick = Math.Min(neededQuantity, product.Quantity);

			//	if (quantityToPick <= 0)
			//	{
			//		return PickingResult.Fail("Brak zapotrzebowania na ten produkt dla wybranego zlecenia.");
			//	}

			//	var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletId));
				
			//	if (virtualPallet == null || virtualPallet.Id == 0)
			//	{
			//		virtualPallet = new VirtualPallet
			//		{
			//			Pallet = pallet,
			//			PalletId = pallet.Id,
			//			DateMoved = DateTime.UtcNow,
			//			LocationId = pallet.LocationId,
			//			IssueInitialQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
			//			Allocations = new List<Allocation>()
			//			// Dodaj inne wymagane pola (np. Status, CreatedAt = DateTime.UtcNow)
			//		};

			//		_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
			//	}
			//	await ReduceAllocationAsync(issue, product.ProductId, quantityToPick, userId);

			//	await ProcessPickingActionAsync(pallet, issue, product.ProductId, quantityToPick, userId);

			//	// Ta logika jest specyficzna dla manuala (tworzenie nowej alokacji)
			//	var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToPick);
			//	_allocationRepo.AddAllocation(newAllocation);

			//	newAllocation.PickingStatus = PickingStatus.Picked;
			//	var historyPicking = new HistoryDataPicking
			//				(
			//					newAllocation.Id,
			//					newAllocation.VirtualPallet.PalletId,
			//					newAllocation.IssueId,
			//						 newAllocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 newAllocation.Quantity,
			//						 0,
			//						 PickingStatus.Available,
			//						 newAllocation.PickingStatus,
			//						 userId,
			//						 DateTime.UtcNow
			//					);
			//	_eventCollector.AddDeferred(async () =>
			//			new CreateHistoryPickingNotification(
			//				new HistoryDataPicking
			//				(
			//					newAllocation.Id,
			//					newAllocation.VirtualPallet.PalletId,
			//					newAllocation.IssueId,
			//						 newAllocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 newAllocation.Quantity,
			//						 0,
			//						 PickingStatus.Available,
			//						 newAllocation.PickingStatus,
			//						 userId,
			//						 DateTime.UtcNow
			//					)));
				
			//	await _werehouseDbContext.SaveChangesAsync();
			//	foreach (var evn in _eventCollector.Events)
			//	{
			//		await _mediator.Publish(evn, CancellationToken.None);
			//	}
			//	foreach (var factory in _eventCollector.DeferredEvents)
			//	{
			//		await _mediator.Publish(await factory());
			//	}

			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	_eventCollector.Clear();
			//	return PickingResult.Ok("Towar dołączono do zlecenia");
			//}
			//catch (PalletException pnfEx)
			//{
			//	await transaction.RollbackAsync();
			//	return PickingResult.Fail(pnfEx.Message);
			//}
			//catch (IssueException onfEx)
			//{
			//	await transaction.RollbackAsync();
			//	return PickingResult.Fail(onfEx.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
			//	return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			//}
		}
		public async Task<PickingResult> ClosePickingPalletAsync(string palletId, int issueId, string userId)
		{
			return await _mediator.Send(new ClosePickingPalletCommand(palletId, issueId, userId));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var pallet = await _palletRepo.GetPalletByIdAsync(palletId) ?? throw new PalletException(palletId);
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueException(palletId);
			//	if (pallet.Status != PalletStatus.Picking) { throw new PalletException($"Palety {pallet.Id} nie można zamknąć. "); }
			//	_pickingPalletRepo.ClosePickingPallet(palletId, issueId);
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	await _mediator.Publish(new CreatePalletOperationNotification(
			//				pallet.Id,
			//				pallet.LocationId,
			//				ReasonMovement.Picking,
			//				userId,
			//				PalletStatus.ToIssue,
			//				null
			//			));
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, userId));
			//	return PickingResult.Ok("Zamknięto paletę");
			//}
			//catch (PalletException exp)
			//{
			//	await transaction.RollbackAsync();
			//	return PickingResult.Fail(exp.Message);
			//}
			//catch (IssueException exo)
			//{
			//	await transaction.RollbackAsync();
			//	return PickingResult.Fail(exo.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	return PickingResult.Fail("Nastąpił nieoczekiwany błąd");
			//}
		}

		// metoda pomocnicza dla Picking - picking helper
		private async Task CreatePalletOrAddToPalletAsync(int issueId, int productId, int quantity, string userId, DateOnly? bestBefore)
		{
			 await _mediator.Send(new CreatePalletOrAddToPalletCommand(issueId, productId, quantity, userId, bestBefore));
			//var filter = new PalletSearchFilter
			//{
			//	IssueId = issueId,
			//	PalletStatus = PalletStatus.Picking,
			//};
			//var oldPallet = await _palletRepo.GetPalletsByFilter(filter).FirstOrDefaultAsync();
			//if (oldPallet == null)
			//{
			//	//pokaż komunikat weź nową paletę
			//	var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
			//	//var sourcePalletBB = sourcePallet.ProductsOnPallet.Single().BestBefore;
			//	var sourcePalletBB = bestBefore;
			//	var pallet = new Pallet
			//	{
			//		Id = newIdPallet,
			//		Status = PalletStatus.Picking,
			//		IssueId = issueId,
			//		LocationId = 100100,//lokalizacja że polu pickingu
			//		DateReceived = DateTime.UtcNow,
			//		ProductsOnPallet = new List<ProductOnPallet>
			//			{
			//				new ProductOnPallet
			//				{
			//					PalletId = newIdPallet,
			//					ProductId = productId,
			//					Quantity = quantity,
			//					DateAdded = DateTime.UtcNow,
			//					BestBefore = sourcePalletBB
			//				}
			//			},
			//	};
			//	_palletRepo.AddPallet(pallet);
			//	_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id,
			//	pallet.LocationId,
			//	ReasonMovement.Picking,
			//	userId,
			//	PalletStatus.Picking,
			//	null));
			//}
			//else
			//{
			//	var pickingPallet = oldPallet;
			//	var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
			//	if (existingProduct != null)
			//	{
			//		existingProduct.Quantity += quantity;
			//	}
			//	else
			//	{
			//		pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
			//		{
			//			ProductId = productId,
			//			Quantity = quantity,
			//			DateAdded = DateTime.UtcNow,
			//		});
			//	}
			//	_eventCollector.Add(new CreatePalletOperationNotification(oldPallet.Id,
			//	oldPallet.LocationId,
			//	ReasonMovement.Picking,
			//	userId,
			//	PalletStatus.Picking,
			//	null));
			//}
		}
		private async Task ProcessPickingActionAsync(Pallet sourcePallet, Issue issue, int productId, int quantityToPick, string userId)
		{
			await _mediator.Send(new ProcessPickingActionCommand(sourcePallet, issue, productId, quantityToPick, userId));
			//var productOnSourcePallet = sourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == productId)
			//	?? throw new PalletException($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			//var bestBefore = productOnSourcePallet.BestBefore;
			//await CreatePalletOrAddToPalletAsync(issue.Id, productId, quantityToPick, userId, bestBefore);
			//productOnSourcePallet.Quantity -= quantityToPick;
			//if (productOnSourcePallet.Quantity == 0)
			//{
			//	sourcePallet.Status = PalletStatus.Archived;
			//	_eventCollector.Add(new CreatePalletOperationNotification(sourcePallet.Id,
			//	sourcePallet.LocationId,
			//	ReasonMovement.Picking,
			//	issue.PerformedBy,
			//	PalletStatus.Archived,
			//	null));
			//}
			//else
			//{
			//	_eventCollector.Add(new CreatePalletOperationNotification(sourcePallet.Id,
			//	sourcePallet.LocationId,
			//	ReasonMovement.Picking,
			//	issue.PerformedBy,
			//	PalletStatus.ToPicking,
			//	null));
			//}
		}
		private async Task ReduceAllocationAsync(Issue issue, int productId, int quantity, string userId)
		{
			await _mediator.Send(new ReduceAllocationCommand(issue, productId, quantity, userId));
			//var allocations = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issue.Id, productId);
			//if (allocations == null) throw new Exception("DB Error");//TODO

			//foreach (var allocation in allocations)
			//{
			//	if (quantity <= 0) break;

			//	if (quantity > 0)
			//	{
			//		if (allocation.Quantity > quantity)
			//		{
			//			allocation.Quantity -= quantity;
			//			quantity = 0;
			//			var historyPicking = new HistoryDataPicking
			//				(
			//					allocation.Id,
			//					allocation.VirtualPallet.PalletId,
			//					allocation.IssueId,
			//						 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 allocation.Quantity,
			//						 0,
			//						 PickingStatus.Correction,
			//						 allocation.PickingStatus,
			//						 userId,
			//						 DateTime.UtcNow
			//					);
			//			_eventCollector.Add(new CreateHistoryPickingNotification(
			//				historyPicking));
			//		}
			//		else
			//		{
			//			quantity -= allocation.Quantity;
			//			allocation.Quantity = 0;
			//			var historyPicking = new HistoryDataPicking
			//				(
			//					allocation.Id,
			//					allocation.VirtualPallet.PalletId,
			//					allocation.IssueId,
			//						 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
			//						 allocation.Quantity,
			//						 0,
			//						 PickingStatus.Correction,
			//						 allocation.PickingStatus,
			//						 userId,
			//						 DateTime.UtcNow
			//					);
			//			_eventCollector.Add(new CreateHistoryPickingNotification(
			//				historyPicking));
			//		}
			//	}
			//}
		}
	}
}









