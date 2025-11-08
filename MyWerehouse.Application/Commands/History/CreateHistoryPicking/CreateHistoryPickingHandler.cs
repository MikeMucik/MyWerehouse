using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Commands.History.CreateHistoryPicking
{
	public class CreateHistoryPickingHandler : INotificationHandler<CreateHistoryPickingCommand>
	{
		private IPickingPalletRepo _pickingPalletRepo;
		private IAllocationRepo _allocationRepo;
		private IHistoryPickingRepo _historyPickingRepo;
		public CreateHistoryPickingHandler(
			IPickingPalletRepo pickingPalletRepo,
			IAllocationRepo allocationRepo,
			IHistoryPickingRepo historyPickingRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_allocationRepo = allocationRepo;
			_historyPickingRepo = historyPickingRepo;
		}
		public async Task Handle(CreateHistoryPickingCommand request, CancellationToken cancellationToken)
		{
			var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(request.VirtualPalletId)
				?? throw new InvalidOperationException($"Brak palety virtualnej {request.VirtualPalletId}.")
				;

			var allocation = await _allocationRepo.GetAllocationAsync(request.AllocationId)
				?? throw new InvalidOperationException($"Brak alokacji {request.AllocationId}.");
			var quantity = request.QuantityPicked;
			if (request.QuantityPicked == 0)
			{
				if (allocation.PickingStatus == PickingStatus.Picked)
				{
					quantity = allocation.Quantity;
				}
			}
			var history = new HistoryPicking
			{
				AllocationId = allocation.Id,
				VirtualPalletId = virtualPallet.Id,
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
		//	await _historyPickingRepo.SaveChanges();

			//return Unit.Value;
		}
	}
}
