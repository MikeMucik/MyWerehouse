using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Commands.History.CreateMoveMent
{
	public class CreatePalletMovementHandler : INotificationHandler<CreatePalletMovementCommand>
	{
		private IPalletRepo _palletRepo;
		private IPalletMovementRepo _palletMovementRepo;
		private ILocationRepo _locationRepo;
		public CreatePalletMovementHandler(IPalletRepo palletRepo, IPalletMovementRepo palletMovementRepo, ILocationRepo locationRepo)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			_locationRepo = locationRepo;
		}
		public async Task Handle(CreatePalletMovementCommand command, CancellationToken cancellationToken)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletId);
			if (pallet == null)
				throw new PalletNotFoundException($"Pallet with ID {command.PalletId} not found.");
			var details = command.Details ?? pallet.ProductsOnPallet
				.Select(p => new PalletMovementDetail
				{
					ProductId = p.ProductId,
					Quantity = p.Quantity,
				}).ToList();
			var destinationLocation = await _locationRepo.GetLocationByIdAsync(command.DestinationLocationId);
			var movement = new PalletMovement
			{
				PalletId = pallet.Id,

				SourceLocationId = command.SourceLocationId,
				SourceLocationSnapShot = $"{pallet.Location.Bay}-{pallet.Location.Aisle}-{pallet.Location.Position}-{pallet.Location.Height}",

				DestinationLocationId = command.DestinationLocationId,
				DestinationLocationSnapShot = $"{destinationLocation.Bay}-{destinationLocation.Aisle}-{destinationLocation.Position}-{destinationLocation.Height}",
				Reason = command.ReasonMovement,
				PerformedBy = command.UserId,
				PalletMovementDetails = details.ToList(),
				MovementDate = DateTime.UtcNow,
				PalletStatus = command.PalletStatus
			};
			await _palletMovementRepo.AddPalletMovementAsync(movement, cancellationToken);

		}
	}
}
