using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;
using MyWerehouse.Application.Pallets.Queries.GetOneAvailablePalletByProduct;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue
{
	public class AddAllocationToIssueNewHandler(IAllocationRepo allocationRepo,
		IEventCollector eventCollector,
		IMediator mediator) : IRequestHandler<AddAllocationToIssueNewCommand, AddAllocationToIssueResult>
	{
		private readonly IAllocationRepo _allocationRepo = allocationRepo;
		private readonly IEventCollector _eventCollector = eventCollector;
		private readonly IMediator _mediator = mediator;
		public async Task<AddAllocationToIssueResult> Handle(AddAllocationToIssueNewCommand command, CancellationToken ct)
		{
			var issue = command.Issue;
			var quantity = command.Rest;
			var listOfAllocation = new List<Allocation>();
			var virtualPallets = command.VirtualPallets;
			foreach (var virtualPallet in virtualPallets)
			{
				var alreadyAllocated = virtualPallet.Allocations.Sum(a => a.Quantity);
				var availableOnThisPallet = virtualPallet.InitialPalletQuantity - alreadyAllocated;
				if (availableOnThisPallet <= 0) continue;
				var quantityToTake = Math.Min(quantity, availableOnThisPallet);
				var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToTake);
				_allocationRepo.AddAllocation(newAllocation);
				listOfAllocation.Add(newAllocation);
				issue.Allocations.Add(newAllocation);
				quantity -= quantityToTake;
				if (quantity <= 0) break;
			}
			var palletsAvailableToPicking = await _mediator.Send(new GetOneAvailablePalletByProductQuery(command.ProductId, command.BestBefore), ct);
			while (quantity > 0)
			{
				
				var newVirtualPallet = await _mediator.Send(new AddPalletToPickingCommand(issue, command.ProductId, command.BestBefore, command.PerfomedBy, command.Pallets), ct);
				if (newVirtualPallet.Success == false)
				{					
					return AddAllocationToIssueResult.Fail(newVirtualPallet.Message); 
				}
				var quantityToTake = Math.Min(quantity, newVirtualPallet.VirtualPallet.InitialPalletQuantity);
				var newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet.VirtualPallet, issue, quantityToTake);
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
								 allocation.ProductId,
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
								 allocation.ProductId,
								 allocation.Quantity,
								 0,
								 PickingStatus.Allocated,
								 allocation.PickingStatus,
								 command.PerfomedBy,
								 DateTime.UtcNow
							)
							));
			}
			return AddAllocationToIssueResult.Ok();
		}
	}
}
