using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Events.CreateMovement
{
	public class PalletChangeStatusHandler : INotificationHandler<ChangeStatusOfPalletNotification>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly ILocationRepo _locationRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public PalletChangeStatusHandler(IPalletRepo palletRepo,
			IPalletMovementRepo palletMovementRepo,
			ILocationRepo locationRepo,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			_locationRepo = locationRepo;
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task Handle(ChangeStatusOfPalletNotification notification, CancellationToken cancellationToken)
		{
			//var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletId)
			//	?? throw new NotFoundPalletException(command.PalletId);
			//var details = command.Details ?? pallet.ProductsOnPallet
			//	.Select(p => new PalletMovementDetail
			//	{
			//		ProductId = p.ProductId,
			//		Quantity = p.Quantity,
			//	}).ToList();
			//var destinationLocation = await _locationRepo.GetLocationByIdAsync(command.DestinationLocationId);
			var movement = new PalletMovement
			{
				PalletId = notification.PalletId,

				SourceLocationId = notification.SourceLocationId,
				SourceLocationSnapShot = notification.SourceSnapshot,
				//$"{pallet.Location.Bay}-{pallet.Location.Aisle}-{pallet.Location.Position}-{pallet.Location.Height}",

				DestinationLocationId = notification.DestinationLocationId,
				DestinationLocationSnapShot = notification.DestinationSnapshot,
				//$"{destinationLocation.Bay}-{destinationLocation.Aisle}-{destinationLocation.Position}-{destinationLocation.Height}",
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
