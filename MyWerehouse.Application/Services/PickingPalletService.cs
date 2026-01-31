using AutoMapper;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet;
using MyWerehouse.Application.PickingPallets.Commands.DoPicking;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet;
using MyWerehouse.Application.PickingPallets.Queries.GetListToPicking;
using MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Commands.ExecuteCorrectedPicking;
using MyWerehouse.Application.PickingPallets.Queries.PrepareCorrectedPicking;

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
		public async Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateOnly dateMovedStart, DateOnly dateMovedEnd)
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
		public async Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateOnly dateIssueStart, DateOnly dateIssueEnd)
		{
			return await _mediator.Send(new GetListIssueToPickingQuery(dateIssueStart, dateIssueEnd));
			//var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			//if (pickingPallets.Count == 0)
			//{
			//	return new List<PickingGuideLineDTO>();
			//}
			//var allNededIssuesIds = pickingPallets
			//	.SelectMany(p => p.PickingTasks)
			//	.Select(i => i.IssueId)
			//	.Distinct()
			//	.ToList();

			//var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);
			//var issueDictionary = allIssues.ToDictionary(i => i.Id);
			//return [.. pickingPallets
			//	.SelectMany(p => p.PickingTasks.Select(a => new
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
		//Lista ile danego towaru dla danej alokacji Product's list by pickingTasks
		public async Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateOnly dateIssueStart, DateOnly dateIssueEnd)
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
			//	.SelectMany(p => p.PickingTasks)
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

			//	var pickingTasks = pallet.PickingTasks;
			//	foreach (var pickingTask in pickingTasks)
			//	{
			//		if (!issueDictionary.TryGetValue(pickingTask.IssueId, out var issue))
			//		{
			//			continue;
			//		}
			//		var clientId = issue.ClientId;
			//		var key = (clientId, pickingTask.IssueId, productId);
			//		if (aggregationDictionary.TryGetValue(key, out var existingRecord))
			//		{
			//			existingRecord.Quantity += pickingTask.Quantity;
			//		}
			//		else
			//		{
			//			var productIssue = new ProductToIssueDTO
			//			{
			//				ClientIdOut = clientId,
			//				IssueId = pickingTask.IssueId,
			//				ProductId = productId,
			//				Quantity = pickingTask.Quantity,
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
		//pokaż alokacje dla palety Show pickingTasks to do - scan pallet
		public async Task<List<PickingTaskDTO>> ShowTaskToDoAsync(string palletSourceScanned, DateTime pickingDate)
		{
			return await _mediator.Send(new ShowTaskToDoQuery(palletSourceScanned, pickingDate));
			//var palletPickingId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletSourceScanned);
			//var pickingTasks = await _pickingTaskRepo.GetPickingTaskListAsync(palletPickingId, pickingDate);
			////mapper??
			//return pickingTasks.Select(pickingTask => new PickingTaskDTO
			//{
			//	PickingTaskId = pickingTask.Id,
			//	IssueId = pickingTask.IssueId,
			//	SourcePalletId = pickingTask.VirtualPallet.Pallet.Id,
			//	ProductId = pickingTask.VirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
			//	PickingStatus = pickingTask.PickingStatus,
			//	RequestedQuantity = pickingTask.Quantity,
			//	BestBefore = pickingTask.VirtualPallet.Pallet.ProductsOnPallet.First().BestBefore
			//}).ToList();
		}
		//faktyczne działanie pickingu - zmiany w bazie Do pick - arranging goods
		public async Task<PickingResult> DoPickingAsync(PickingTaskDTO pickingTaskDTO, string userId)
		{
			return await _mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, userId));
		}

		public async Task<PrepareCorrectedPickingResult> PrepareManualPickingAsync(string palletId)
		{
			return await _mediator.Send(new PrepareCorrectedPickingQuery(palletId));
		}
		public async Task<PickingResult> ExecuteManualPickingAsync(string palletId, int issueId, string userId)
		{
			return await _mediator.Send(new ExecuteCorrectedPickingCommand(palletId, issueId, userId));
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
	}
}









