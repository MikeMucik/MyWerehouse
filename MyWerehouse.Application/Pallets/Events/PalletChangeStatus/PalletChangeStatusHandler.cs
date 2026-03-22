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

namespace MyWerehouse.Application.Pallets.Events.CreateMovement
{
	public class PalletChangeStatusHandler(IPalletMovementRepo palletMovementRepo) : INotificationHandler<ChangeStatusOfPalletNotification>
	{
		private readonly IPalletMovementRepo _palletMovementRepo = palletMovementRepo;

		public async Task Handle(ChangeStatusOfPalletNotification notification, CancellationToken cancellationToken)
		{			
			var movement = new PalletMovement
			{
				//Id = notification.PalletId,
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
				
				PalletMovementDetails = notification.Details
				.Select(d => new PalletMovementDetail
				 {
					 ProductId = d.ProductId,
					 Quantity = d.Quantity,
				 })
				.ToList(),
			};
			await _palletMovementRepo.AddPalletMovementAsync(movement, cancellationToken);
		}
	}
}
