using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Pallets.Events.CreateOperation
{
	public class CreatePalletOperationHandler : INotificationHandler<CreatePalletOperationNotification>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public CreatePalletOperationHandler(IPalletRepo palletRepo,
			IPalletMovementRepo palletMovementRepo,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task Handle(CreatePalletOperationNotification request, CancellationToken cancellationToken)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId)??
				throw new PalletException($"Pallet with ID {request.PalletId} not found.");
			var details = request.Details??pallet.ProductsOnPallet
				.Select(p=> new PalletMovementDetail
				{
					ProductId = p.ProductId,
					Quantity = p.Quantity,
				}).ToList();			
			
			var movement = new PalletMovement
			{
				PalletId = pallet.Id,				
				DestinationLocationId =request.DestinationLocationId,
				DestinationLocationSnapShot = $"{pallet.Location.Bay}-{pallet.Location.Aisle}-{pallet.Location.Position}-{pallet.Location.Height}",
				Reason =request.ReasonMovement,
				PerformedBy =request.UserId,
				PalletMovementDetails = details.ToList(),
				MovementDate = DateTime.UtcNow,
				PalletStatus =request.PalletStatus
			};
			await _palletMovementRepo.AddPalletMovementAsync(movement, cancellationToken);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);		
		}
	}
}
