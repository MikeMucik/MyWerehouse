using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.ReduceAllocation
{
	public class ReduceAllocationHandler(IAllocationRepo allocationRepo,
		IEventCollector eventCollector) : IRequestHandler<ReduceAllocationCommand, Unit>
	{
		private readonly IAllocationRepo _allocationRepo = allocationRepo;
		private readonly IEventCollector _eventCollector = eventCollector;

		public async Task<Unit> Handle (ReduceAllocationCommand request, CancellationToken ct)
		{
			var allocations = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(request.Issue.Id, request.ProductId);
			if (allocations == null) throw new Exception("DB Error");//TODO
			var quantity = request.Quantity;
			foreach (var allocation in allocations)
			{
				
				if (quantity <= 0) break;

				if (quantity > 0)
				{
					if (allocation.Quantity > quantity)
					{
						allocation.Quantity -= quantity;
						quantity = 0;
						var historyPicking = new HistoryDataPicking
							(
								allocation.Id,
								allocation.VirtualPallet.PalletId,
								allocation.IssueId,
									 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 allocation.Quantity,
									 0,
									 PickingStatus.Correction,
									 allocation.PickingStatus,
									request.UserId,
									 DateTime.UtcNow
								);
						_eventCollector.Add(new CreateHistoryPickingNotification(
							historyPicking));
					}
					else
					{
						quantity -= allocation.Quantity;
						allocation.Quantity = 0;
						var historyPicking = new HistoryDataPicking
							(
								allocation.Id,
								allocation.VirtualPallet.PalletId,
								allocation.IssueId,
									 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 allocation.Quantity,
									 0,
									 PickingStatus.Correction,
									 allocation.PickingStatus,
									request.UserId,
									 DateTime.UtcNow
								);
						_eventCollector.Add(new CreateHistoryPickingNotification(
							historyPicking));
					}
				}
			}return Unit.Value;
		}
	}
}
