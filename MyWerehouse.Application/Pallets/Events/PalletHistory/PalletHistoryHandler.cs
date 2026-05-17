using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Events;

namespace MyWerehouse.Application.Pallets.Events.PalletHistory
{
	public class PalletHistoryHandler(IHistoryPalletRepo palletMovementRepo) : INotificationHandler<PalletHistoryNotification>
	{
		private readonly IHistoryPalletRepo _palletMovementRepo = palletMovementRepo;

		public Task Handle(PalletHistoryNotification notification, CancellationToken cancellationToken)
		{			
			var movement = new HistoryPallet
			{
				PalletId = notification.PalletId,
				PalletNumber = notification.PalletNumber,
				SourceLocationId = notification.SourceLocationId,
				SourceLocationSnapShot = notification.SourceSnapshot,
				DestinationLocationId = notification.DestinationLocationId,
				DestinationLocationSnapShot = notification.DestinationSnapshot,
				Reason = notification.ReasonMovement,
				PerformedBy = notification.UserId,
				MovementDate = DateTime.UtcNow,
				PalletStatus = notification.PalletStatus,
				
				HistoryPalletDetails = notification.Details
				.Select(d => new HistoryPalletDetail
				 {
					 ProductId = d.ProductId,
					 Quantity = d.Quantity,
				 })
				.ToList(),
			};
			_palletMovementRepo.AddHistoryPallet(movement);
			return Task.CompletedTask;
		}
	}
}
