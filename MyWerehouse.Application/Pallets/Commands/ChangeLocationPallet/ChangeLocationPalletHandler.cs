using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet
{
	public class ChangeLocationPalletHandler(IPalletRepo palletRepo,
		ILocationRepo locationRepo,
		WerehouseDbContext werehouseDbContext,
		IMediator mediator) : IRequestHandler<ChangeLocationPalletCommand, ChangeLocationResults>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IMediator _mediator = mediator;

		public async Task<ChangeLocationResults> Handle(ChangeLocationPalletCommand request, CancellationToken ct)
		{
			//TODO Figure out change pallet's status when operator set location
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId) ?? throw new NotFoundPalletException(request.PalletId);
				//location is occupied?
				//tu front musi przy pomocy backanedu wyliczyć locationId, frontend must find locationdId by data from a form
				if (request.DestinationLocationId <= 0)
					return new ChangeLocationResults
					{
						Success = false,
						Message = "niprawidłowa lokalizacja."
					};
				var existingPalletInDestination = await _palletRepo.CheckOccupancyAsync(request.DestinationLocationId);
				var location = await _locationRepo.GetLocationByIdAsync(request.DestinationLocationId);
				var fullNameLocation = $" Bay = {location.Bay} Aisle = {location.Aisle} Position = {location.Position} Height ={location.Height}";
				if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id && !request.Force)
				{
					return new ChangeLocationResults
					{
						Success = false,
						RequiresConfirmation = true,
						Message = $"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}."
						//OccupiedByPalletId = existingPalletInDestination.Id // Opcjonalnie: Dodaj pole do Results (frontend pokaże)
					};
				}
				var destLocation = await _locationRepo.GetLocationByIdAsync(request.DestinationLocationId);
				pallet.MoveToLocation(destLocation, request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return new ChangeLocationResults
				{
					Success = true,
					RequiresConfirmation = false,
					Message = $"Paleta {pallet.Id} została umieszczona w lokalizacji. "
				};
			}
			catch (NotFoundPalletException pe)
			{
				return new ChangeLocationResults
				{
					Success = false,
					RequiresConfirmation = false,
					Message = pe.Message
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				throw;
			}
		}
	}
}
