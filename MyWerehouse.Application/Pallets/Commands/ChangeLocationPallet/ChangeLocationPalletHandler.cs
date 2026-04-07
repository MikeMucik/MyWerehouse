using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet
{
	public class ChangeLocationPalletHandler(IPalletRepo palletRepo,
		ILocationRepo locationRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<ChangeLocationPalletCommand, AppResult<ChangeLocationResults>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		public async Task<AppResult<ChangeLocationResults>> Handle(ChangeLocationPalletCommand request, CancellationToken ct)
		{
			//TODO Figure out change pallet's status when operator set location
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
				if (pallet == null) return AppResult<ChangeLocationResults>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
				//location is occupied?
				//var oldDestiantion = pallet.Location;
				//var oldDestiantionId= oldDestiantion.Id;
				//var oldSnapShot = oldDestiantion.ToSnopShot();
				//tu front musi przy pomocy backanedu wyliczyć locationId, frontend must find locationdId by data from a form
				if (request.DestinationLocationId <= 0)
					return AppResult<ChangeLocationResults>.Fail("niprawidłowa lokalizacja.", ErrorType.NotFound);

				var existingPalletInDestination = await _palletRepo.CheckOccupancyAsync(request.DestinationLocationId);
				var location = await _locationRepo.GetLocationByIdAsync(request.DestinationLocationId);
				if (location == null)
				{
					return AppResult<ChangeLocationResults>.Fail($"Brak lokalizacji o numerze {request.DestinationLocationId}.", ErrorType.NotFound);
				}
				var fullNameLocation = $" Bay = {location.Bay} Aisle = {location.Aisle} Position = {location.Position} Height ={location.Height}";
				if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id && !request.Force)
				{
					var answerWhenOccupied = new ChangeLocationResults
					{
						Success = false,
						RequiresConfirmation = true,
						Message = $"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}."
						//OccupiedByPalletId = existingPalletInDestination.Id // Opcjonalnie: Dodaj pole do Results (frontend pokaże)
					};
					return AppResult<ChangeLocationResults>.Success(answerWhenOccupied, answerWhenOccupied.Message);
				}
				//var destLocation = await _locationRepo.GetLocationByIdAsync(request.DestinationLocationId);
				//if (location == null)
				//{
				//	return AppResult<ChangeLocationResults>.Fail($"Brak lokalizacji o numerze {request.DestinationLocationId}.", ErrorType.NotFound);
				//}
				var snapShot = location.ToSnopShot();
				pallet.MoveToLocation(location.Id,snapShot, request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);

				var answerWhenFree = new ChangeLocationResults
				{
					Success = true,
					RequiresConfirmation = false,
					Message = $"Paleta {pallet.Id} została umieszczona w lokalizacji. "
				};
				return AppResult<ChangeLocationResults>.Success(answerWhenFree, answerWhenFree.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				throw;
			}
		}
	}
}
