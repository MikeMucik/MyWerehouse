using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Commands.ProcessPickingAction;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.DoPicking
{
	public class DoPickingHandler : IRequestHandler<DoPickingCommand, PickingResult>
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
		public async Task<PickingResult> Handle(DoPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{				
				var newAllocation = new Allocation();
				var allocationToChange = await _allocationRepo.GetAllocationAsync(request.AllocationDTO.AllocationId);
				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(allocationToChange.VirtualPalletId);
				var issueId = allocationToChange.IssueId;
				var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new NotFoundIssueException(issueId);
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.AllocationDTO.SourcePalletId)
					?? throw new NotFoundPalletException(request.AllocationDTO.SourcePalletId);
				if (issue.IssueStatus == IssueStatus.Pending) { issue.IssueStatus = IssueStatus.InProgress; }
				var neededQuantity = request.AllocationDTO.RequestedQuantity;
				var pickedQuantity = request.AllocationDTO.PickedQuantity;
				var completion = PickingCompletion.Full;
				if (pickedQuantity <= 0 || pickedQuantity > neededQuantity)
				{
					await transaction.RollbackAsync(ct);
					return PickingResult.Fail("Operacja nie dozwolona");
				}
				if (neededQuantity > pickedQuantity)
				{
					completion = PickingCompletion.Partial;
				}

				await _mediator.Send(new ProcessPickingActionCommand(sourcePallet, issue, request.AllocationDTO.ProductId,
						request.AllocationDTO.PickedQuantity, request.UserId, allocationToChange, completion), ct);
				var historyPicking = new HistoryDataPicking(allocationToChange.Id, allocationToChange.VirtualPallet.PalletId,
							allocationToChange.IssueId, allocationToChange.ProductId, allocationToChange.Quantity,
							request.AllocationDTO.PickedQuantity, PickingStatus.Allocated, allocationToChange.PickingStatus,
							request.UserId, DateTime.UtcNow);
				_eventCollector.Add(new CreateHistoryPickingNotification(historyPicking));

				if (neededQuantity == pickedQuantity)
				{
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					foreach (var evn in _eventCollector.Events)
					{
						await _mediator.Publish(evn, CancellationToken.None);
					}
					return PickingResult.Ok("Towar dołączono do zlecenia");
				}
				else
				{
					var newQuantityToAllocation = neededQuantity - pickedQuantity;
					var	newVirtualPallet = await _mediator.Send(new AddPalletToPickingCommand(issue, request.AllocationDTO.ProductId,
						request.AllocationDTO.BestBefore, request.UserId, []), ct);
					if (newVirtualPallet.Success == false) 
					{
						await transaction.RollbackAsync(ct);
						return PickingResult.Fail(newVirtualPallet.Message); 
					}
					newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet.VirtualPallet, issue, newQuantityToAllocation);
					_allocationRepo.AddAllocation(newAllocation);
					var newHistoryPicking = new HistoryDataPicking(newAllocation.Id, newAllocation.VirtualPallet.PalletId,
									newAllocation.IssueId, newAllocation.ProductId, newAllocation.Quantity,
									request.AllocationDTO.PickedQuantity, PickingStatus.Allocated,
									newAllocation.PickingStatus, request.UserId, DateTime.UtcNow);
					_eventCollector.Add(new CreateHistoryPickingNotification(newHistoryPicking));
					//pallet lock with non-conformity 
					sourcePallet.Status = PalletStatus.OnHold;
					_eventCollector.Add(new CreatePalletOperationNotification(sourcePallet.Id, sourcePallet.LocationId,
						ReasonMovement.Correction, request.UserId, PalletStatus.OnHold,	null));

					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					foreach (var evn in _eventCollector.Events)
					{
						await _mediator.Publish(evn, CancellationToken.None);
					}
					return PickingResult.Ok("Towar dołączono do zlecenia, stworzono dodatkowe zadanie do pickingu.");
				}				
			}
			catch (NotFoundPalletException pnfEx)
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
