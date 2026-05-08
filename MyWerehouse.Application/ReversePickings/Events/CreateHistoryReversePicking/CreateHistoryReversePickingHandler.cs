using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Events;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking
{
	public class CreateHistoryReversePickingHandler : INotificationHandler<CreateHistoryReversePickingNotification>
	{		
		private readonly IHistoryReversePickingRepo _historyReversePickingRepo;
		private readonly IPalletRepo _palletRepo;
		public CreateHistoryReversePickingHandler(IHistoryReversePickingRepo historyReversePickingRepo, IPalletRepo pallet)
		{
			_historyReversePickingRepo = historyReversePickingRepo;
			_palletRepo = pallet;
		}
		public async Task Handle(CreateHistoryReversePickingNotification notification, CancellationToken ct)
		{
			var sourceTask = notification.PalletSourceId != null
				? _palletRepo.GetPalletByIdAsync(notification.PalletSourceId.Value)
				: Task.FromResult<Pallet?>(null);
			var destinationTask = notification.PalletDestinationId != null
				? _palletRepo.GetPalletByIdAsync(notification.PalletDestinationId.Value)
				: Task.FromResult<Pallet?>(null);
			var pickingTask = _palletRepo.GetPalletByIdAsync(notification.PickingPalletId);
			await Task.WhenAll(sourceTask, destinationTask, pickingTask);
			//Pallet? sourcePallet = null;
			//if (notification.PalletSourceId != null)
			//{
			//	sourcePallet = await _palletRepo.GetPalletByIdAsync(notification.PalletSourceId.Value);
			//}
			//Pallet? destinationPallet = null;
			//if (notification.PalletDestinationId != null)
			//{
			//	destinationPallet = await _palletRepo.GetPalletByIdAsync(notification.PalletDestinationId.Value);
			//}
			//Pallet pickingPallet = await _palletRepo.GetPalletByIdAsync(notification.PickingPalletId);
			var sourcePallet = sourceTask.Result;
			var destinationPallet = destinationTask.Result;
			var pickingPallet = pickingTask.Result;
				var history = new HistoryReversePicking
				{
					ReversePickingId = notification.ReversePickingId,
					PickingPalletId = notification.PickingPalletId,
					PickingPalletNumber = pickingPallet.PalletNumber,
					PalletSourceId = notification.PalletSourceId,
					PalletSourceNumber = sourcePallet?.PalletNumber,
					PalletDestinationId = notification.PalletDestinationId,
					PalletDestinationNumber = destinationPallet?.PalletNumber,
					IssueId = notification.IssueId,
					IssueNumber = notification.IssueNumber,
					ProductId = notification.ProductId,
					DateTime = DateTime.UtcNow,
					PerformedBy = notification.UserId,
					Quantity = notification.Quantity,
					StatusBefore = notification.StatusBefore,
					StatusAfter = notification.StatusAfter,
				};
			_historyReversePickingRepo.AddHistoryReversePicking(history);
		}
	}
}
