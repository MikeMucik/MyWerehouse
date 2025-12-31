using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue
{
	public class AddAllocationToIssueNewHandler(IAllocationRepo allocationRepo,
		IEventCollector eventCollector,
		IMediator mediator) :IRequestHandler<AddAllocationToIssueNewCommand, Unit>
	{
		private readonly IAllocationRepo _allocationRepo = allocationRepo;
		private readonly IEventCollector _eventCollector = eventCollector;
		private readonly IMediator _mediator = mediator;
		public async Task<Unit> Handle(AddAllocationToIssueNewCommand command, CancellationToken ct)
		{
			var issue = command.Issue;			
			var quantity = command.Rest;
			var listOfAllocation = new List<Allocation>();
			var virtualPallets = command.VirtualPallets;
			foreach (var virtualPallet in virtualPallets)
			{
				var alreadyAllocated = virtualPallet.Allocations.Sum(a => a.Quantity);
				var availableOnThisPallet = virtualPallet.IssueInitialQuantity - alreadyAllocated;
				if (availableOnThisPallet <= 0) continue;
				var quantityToTake = Math.Min(quantity, availableOnThisPallet);
				var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToTake);
				_allocationRepo.AddAllocation(newAllocation);
				listOfAllocation.Add(newAllocation);
				issue.Allocations.Add(newAllocation);
				quantity -= quantityToTake;
				if (quantity <= 0) break;
			}
			while (quantity > 0)
			{
				var newVirtualPallet = await _mediator.Send(new AddPalletToPickingCommand(issue, command.ProductId, command.BestBefore, command.PerfomedBy, command.Pallets), ct);
				var quantityToTake = Math.Min(quantity, newVirtualPallet.IssueInitialQuantity);
				var newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, quantityToTake);
				_allocationRepo.AddAllocation(newAllocation);
				listOfAllocation.Add(newAllocation);
				quantity -= quantityToTake;
			}
			foreach (var allocation in listOfAllocation)
			{
				var historyPicking = new HistoryDataPicking
						(
							allocation.Id,
							allocation.VirtualPallet.PalletId,
							allocation.IssueId,
								 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
								 allocation.Quantity,
								 0,
								 PickingStatus.Allocated,
								 allocation.PickingStatus,
								 command.PerfomedBy,
								 DateTime.UtcNow
							);
				_eventCollector.AddDeferred(async () =>
						new CreateHistoryPickingNotification(
							new HistoryDataPicking
						(
							allocation.Id,
							allocation.VirtualPallet.PalletId,
							allocation.IssueId,
								 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
								 allocation.Quantity,
								 0,
								 PickingStatus.Allocated,
								 allocation.PickingStatus,
								 command.PerfomedBy,
								 DateTime.UtcNow
							)
							));
			}
			return Unit.Value;
		}
	}
}
