using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;

namespace MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue
{
	public class AddAllocationToIssueHandler : IRequestHandler<AddAllocationToIssueCommand, List<Allocation>>
	{
		private readonly IAllocationRepo _allocationRepo;
		private readonly IEventCollector _eventCollector;
		private readonly IMediator _mediator;
		public AddAllocationToIssueHandler(
			IAllocationRepo allocationRepo,
			IEventCollector eventCollector,
			IMediator mediator)
		{
			_allocationRepo = allocationRepo;
			_eventCollector = eventCollector;
			_mediator = mediator;
		}
		public async Task<List<Allocation>> Handle(AddAllocationToIssueCommand request, CancellationToken ct)
		{
			var issue = request.Issue;
			var quantity = request.Rest;
			if (quantity <= 0) return [];
			var listOfAllocation = new List<Allocation>();
			var virtualPallets = request.VirtualPallets;
			var xxxx = issue.Allocations.Where(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == request.ProductId).ToList();
			if (issue.Id > 0 && xxxx.Count > 0)
			{
				var oldAllocations = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issue.Id, request.ProductId);
				foreach (var oldAllocation in oldAllocations)
				{
					if (oldAllocation.VirtualPallet.RemainingQuantity >= quantity)
					{
						oldAllocation.Quantity += quantity;
						listOfAllocation.Add(oldAllocation);
						oldAllocation.PickingStatus = PickingStatus.Correction;
					}
				}
				foreach (var allocation in listOfAllocation)
				{
					_eventCollector.AddDeferred(async () =>
							new CreateHistoryPickingNotification(
								allocation.VirtualPalletId,
								allocation.Id,
								request.PerfomedBy,
								PickingStatus.Allocated,
								0));
				}
				return listOfAllocation;
			}
			else
			{
				foreach (var virtualPallet in virtualPallets)
				{
					var alreadyAllocated = virtualPallet.Allocations.Sum(a => a.Quantity);
					var availableOnThisPallet = virtualPallet.IssueInitialQuantity - alreadyAllocated;
					if (availableOnThisPallet <= 0) continue;
					var quantityToTake = Math.Min(quantity, availableOnThisPallet);
					var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToTake);
					_allocationRepo.AddAllocation(newAllocation);
					//czy tu dopisać do issue ?
					listOfAllocation.Add(newAllocation);
					issue.Allocations.Add(newAllocation);
					quantity -= quantityToTake;
					if (quantity <= 0) break;
				}
				while (quantity > 0)
				{
					var newVirtualPallet = await _mediator.Send(new AddPalletToPickingCommand(issue, request.ProductId, request.BestBefore, request.PerfomedBy, request.Pallets), ct);
					var quantityToTake = Math.Min(quantity, newVirtualPallet.IssueInitialQuantity);
					var newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, quantityToTake);
					_allocationRepo.AddAllocation(newAllocation);
					listOfAllocation.Add(newAllocation);
					quantity -= quantityToTake;
				}
				foreach (var allocation in listOfAllocation)
				{
					_eventCollector.AddDeferred(async () =>
							new CreateHistoryPickingNotification(
								allocation.VirtualPalletId,
								allocation.Id,
								request.PerfomedBy,
								PickingStatus.Available,
								0));
				}
				return listOfAllocation;
			}
		}
	}
}
