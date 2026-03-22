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
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IHistoryReversePickingRepo _historyReversePickingRepo;
		private readonly IPalletRepo _palletRepo;
		public CreateHistoryReversePickingHandler(WerehouseDbContext werehouseDbContext,
			IHistoryReversePickingRepo historyReversePickingRepo, IPalletRepo pallet)
		{
			_werehouseDbContext = werehouseDbContext;
			_historyReversePickingRepo = historyReversePickingRepo;
			_palletRepo = pallet;
		}
		public async Task Handle(CreateHistoryReversePickingNotification notification, CancellationToken ct)
		{
			Pallet? sourcePallet = null;
			if (notification.PalletSourceId != null)
			{
				sourcePallet = await _palletRepo.GetPalletByIdAsync(notification.PalletSourceId.Value);
			}
			Pallet? destinationPallet = null;
			if (notification.PalletDestinationId != null)
			{
				destinationPallet = await _palletRepo.GetPalletByIdAsync(notification.PalletDestinationId.Value);
			}
			
				var history = new HistoryReversePicking
				{
					ReversePickingId = notification.ReversePickingId,
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
			await _historyReversePickingRepo.AddHistoryReversePickingAsync(history, ct);
		}
	}
}
