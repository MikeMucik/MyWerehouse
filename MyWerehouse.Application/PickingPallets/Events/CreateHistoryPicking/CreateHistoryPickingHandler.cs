using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking
{
	public class CreateHistoryPickingHandler : INotificationHandler<CreateHistoryPickingNotification>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly IHistoryPickingRepo _historyPickingRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public CreateHistoryPickingHandler(
			IPickingPalletRepo pickingPalletRepo,
			IAllocationRepo allocationRepo,
			IHistoryPickingRepo historyPickingRepo,
			WerehouseDbContext werehouseDbContext
			)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_allocationRepo = allocationRepo;
			_historyPickingRepo = historyPickingRepo;
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task Handle(CreateHistoryPickingNotification request, CancellationToken cancellationToken)
		{
			var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(request.VirtualPalletId)
				?? throw new InvalidOperationException($"Brak palety virtualnej {request.VirtualPalletId}.");

			var allocation = await _allocationRepo.GetAllocationAsync(request.AllocationId)
				?? throw new InvalidOperationException($"Brak alokacji {request.AllocationId}.");
			var quantity = request.QuantityPicked;
			if (request.QuantityPicked == 0) // Maybe Error
			{
				if (allocation.PickingStatus == PickingStatus.Picked)
				{
					quantity = allocation.Quantity;
				}
			}
			var history = new HistoryPicking
			{
				AllocationId = allocation.Id,
				PalletId = allocation.VirtualPallet.PalletId,
				IssueId = allocation.IssueId,
				ProductId = virtualPallet.Pallet.ProductsOnPallet.First().ProductId,
				QuantityAllocated = allocation.Quantity,
				QuantityPicked = quantity,
				StatusBefore = request.StatusBefore,
				StatusAfter = allocation.PickingStatus,
				PerformedBy = request.PerformedBy,
				DateTime = DateTime.UtcNow,
			};
			await _historyPickingRepo.AddHistoryPickingAsync(history, cancellationToken);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
