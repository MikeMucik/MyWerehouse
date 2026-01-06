using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Commands.ProcessPickingAction;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.DoPicking
{
	public class DoPickingHandler:IRequestHandler<DoPickingCommand, PickingResult>
	{
		private readonly IAllocationRepo _allocationRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IIssueRepo _issueRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		public DoPickingHandler(IAllocationRepo allocationRepo,
			IPalletRepo palletRepo,
			IIssueRepo issueRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator,
			IEventCollector eventCollector)
		{
			_allocationRepo = allocationRepo;
			_palletRepo = palletRepo;
			_issueRepo = issueRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
			_eventCollector = eventCollector;
		}
		public async Task<PickingResult> Handle (DoPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var newVirtualPallet = new VirtualPallet();
				var newAllocation = new Allocation();
				var allocationToChange = await _allocationRepo.GetAllocationAsync( request.AllocationDTO.AllocationId);
				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(allocationToChange.VirtualPalletId);
				var issueId = allocationToChange.IssueId;
				var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new NotFoundIssueException(issueId);
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.AllocationDTO.SourcePalletId)
					?? throw new PalletException(request.AllocationDTO.SourcePalletId);
				await _mediator.Send(new ProcessPickingActionCommand(sourcePallet, issue, request.AllocationDTO.ProductId, request.AllocationDTO.PickedQuantity, request.UserId), ct);
				//await ProcessPickingActionAsync(sourcePallet, issue, allocationDTO.ProductId, allocationDTO.PickedQuantity, userId);
				if (request.AllocationDTO.RequestedQuantity ==request.AllocationDTO.PickedQuantity)
				{
					allocationToChange.PickingStatus = PickingStatus.Picked;
					var historyPicking = new HistoryDataPicking
							(
								allocationToChange.Id,
								allocationToChange.VirtualPallet.PalletId,
								allocationToChange.IssueId,
									 allocationToChange.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 allocationToChange.Quantity,
									 0,
									 PickingStatus.Allocated,
									 allocationToChange.PickingStatus,
									request.UserId,
									 DateTime.UtcNow
								);
					_eventCollector.Add(new CreateHistoryPickingNotification(
							historyPicking
							));
				}
				else if (request.AllocationDTO.RequestedQuantity >request.AllocationDTO.PickedQuantity)
				{
					var newQuantityToAllocation =request.AllocationDTO.RequestedQuantity -request.AllocationDTO.PickedQuantity;

					newVirtualPallet = await _mediator.Send(new AddPalletToPickingCommand(issue, request.AllocationDTO.ProductId, request.AllocationDTO.BestBefore,request.UserId, []), ct);
					newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, newQuantityToAllocation);
					_allocationRepo.AddAllocation(newAllocation);
					var historyPicking = new HistoryDataPicking
							(
								newAllocation.Id,
								newAllocation.VirtualPallet.PalletId,
								newAllocation.IssueId,
									 newAllocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 newAllocation.Quantity,
									 0,
									 PickingStatus.Allocated,
									 newAllocation.PickingStatus,
								request.UserId,
									 DateTime.UtcNow
								);
					_eventCollector.Add(new CreateHistoryPickingNotification(
						historyPicking));

					//zablokowanie palety źródłowej bo się nie zgadza stan fizyczny/system
					sourcePallet.Status = PalletStatus.OnHold;
					_eventCollector.Add(new CreatePalletOperationNotification(sourcePallet.Id,
						sourcePallet.LocationId,
						ReasonMovement.Correction,
						request.UserId,
						PalletStatus.OnHold,
						null));
				}

				if (issue.IssueStatus == IssueStatus.Pending) { issue.IssueStatus = IssueStatus.InProgress; }
				//czy to ma sens

				if (request.AllocationDTO.RequestedQuantity == request.AllocationDTO.PickedQuantity)
				{
					var historyPicking = new HistoryDataPicking
							(
								allocationToChange.Id,
								allocationToChange.VirtualPallet.PalletId,
								allocationToChange.IssueId,
									 allocationToChange.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 allocationToChange.Quantity,
									 allocationToChange.Quantity,
									 PickingStatus.Allocated,
									 allocationToChange.PickingStatus,
									request.UserId,
									 DateTime.UtcNow
								);
					await _mediator.Publish(new CreateHistoryPickingNotification(historyPicking), ct);
				}

				await _werehouseDbContext.SaveChangesAsync(ct);
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, CancellationToken.None);
				}
				//_eventCollector.Clear();
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				
				return PickingResult.Ok("Towar dołączono do zlecenia");
			}
			catch (PalletException pnfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(pnfEx.Message);
			}
			catch (NotFoundIssueException onfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(onfEx.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
