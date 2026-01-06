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
using MyWerehouse.Application.Pallets.Commands.AddPalletToPicking;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue
{
	public class AddAllocationToIssueHandler(
		IAllocationRepo allocationRepo,
		IEventCollector eventCollector,
		IMediator mediator,
		IProductRepo productRepo) : IRequestHandler<AddAllocationToIssueCommand, List<Allocation>>
	{
		private readonly IAllocationRepo _allocationRepo = allocationRepo;
		private readonly IEventCollector _eventCollector = eventCollector;
		private readonly IMediator _mediator = mediator;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<List<Allocation>> Handle(AddAllocationToIssueCommand request, CancellationToken ct)
		{
			var issue = request.Issue;
			var targetQuantity = request.Rest;
			var quantity = request.Rest;
			//if (quantity <= 0) return [];
			var listOfAllocation = new List<Allocation>();
			var virtualPallets = request.VirtualPallets;
			var existingAllocations = issue.Allocations.Where(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == request.ProductId).ToList();
			if (targetQuantity == 0)
			{
				if (existingAllocations.Count > 0)
				{
					foreach (var alloc in existingAllocations)
					{
						// Usuwamy z bazy i z listy issue
						_allocationRepo.DeleteAllocation(alloc); // Zakładam że masz taką metodę lub Remove
						issue.Allocations.Remove(alloc);

						// Event Cancelled
						_eventCollector.AddDeferred(async () =>
							new CreateHistoryPickingNotification(
								new HistoryDataPicking
							(
								alloc.Id,
								alloc.VirtualPallet.PalletId,
								alloc.IssueId,
									 alloc.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 alloc.Quantity,
									 0,
									 PickingStatus.Allocated,
									 alloc.PickingStatus,
									 request.PerfomedBy,
									 DateTime.UtcNow

								)));
					}
				}
				return new List<Allocation>(); // Zwracamy pustą listę
			}
			if (existingAllocations.Count > 0)
			{
				foreach (var oldAllocation in existingAllocations)
				{
					if (oldAllocation.VirtualPallet.RemainingQuantity >= quantity)
					{
						oldAllocation.Quantity += quantity;
						listOfAllocation.Add(oldAllocation);
						oldAllocation.PickingStatus = PickingStatus.Correction;

					}
					else
					{
						var allocation = new Allocation
						{
							VirtualPallet = oldAllocation.VirtualPallet,
							Quantity = quantity,
							Issue = oldAllocation.Issue,
							PickingStatus = PickingStatus.Allocated
						};
						_allocationRepo.AddAllocation(allocation);
						listOfAllocation.Add(allocation);
					}
				}
				foreach (var allocation in listOfAllocation)
				{

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
									 PickingStatus.Available,
									 allocation.PickingStatus,
									 request.PerfomedBy,
									 DateTime.UtcNow

								)));
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
									 request.PerfomedBy,
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
									 request.PerfomedBy,
									 DateTime.UtcNow
								)
								));
				}
				return listOfAllocation;
			}
		}
	}
}
