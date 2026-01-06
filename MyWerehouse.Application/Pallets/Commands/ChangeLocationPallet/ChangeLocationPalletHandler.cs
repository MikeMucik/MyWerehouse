using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Events.CreateMovement;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet
{
	public class ChangeLocationPalletHandler(IPalletRepo palletRepo,
		ILocationRepo locationRepo,
		WerehouseDbContext werehouseDbContext,
		IMediator mediator,
		IEventCollector _eventCollector) : IRequestHandler<ChangeLocationPalletCommand, ChangeLocationResults>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IMediator _mediator = mediator;
		private readonly IEventCollector _eventCollector = _eventCollector;

		public async Task<ChangeLocationResults> Handle(ChangeLocationPalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId) ?? throw new PalletException(request.PalletId);
				//sprawdzenie czy lokalizacja jest zajęta
				//tu front musi przy pomocy backanedu wyliczyć locationId
				if (request.DestinationLocationId <= 0)
					return new ChangeLocationResults
					{
						Success = false,
						Message = "niprawidłowa lokalizacja."
					};
				var existingPalletInDestination = await _palletRepo.CheckOccupancyAsync(request.DestinationLocationId); // Nowa metoda repo
				var location = await _locationRepo.GetLocationByIdAsync(request.DestinationLocationId);
				var fullNameLocation = $" Bay = {location.Bay} Aisle = {location.Aisle} Position = {location.Position} Height ={location.Height}";
				if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id && !request.Force) // Jeśli lokalizacja jest zajęta przez inną paletę
				{
					return new ChangeLocationResults
					{
						Success = false,
						RequiresConfirmation = true,
						Message = $"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}."
						//OccupiedByPalletId = existingPalletInDestination.Id // Opcjonalnie: Dodaj pole do Results (frontend pokaże)
					};
				}
				//var destinationLocation = await _locationRepo.GetLocationByIdAsync(request.DestinationLocationId);
				_eventCollector.Add
					(new CreatePalletMovementNotification(
					pallet.Id,
					pallet.LocationId,
					request.DestinationLocationId,
					ReasonMovement.Moved,
				request.UserId,
					pallet.Status,
					null));
				pallet.LocationId = request.DestinationLocationId;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(_eventCollector.Events.First(), ct);
				//_eventCollector.Clear();
				return new ChangeLocationResults
				{
					Success = true,
					RequiresConfirmation = false,
					Message = $"Paleta {pallet.Id} została umieszczona w lokalizacji. "
				};
			}
			catch(PalletException pe)
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
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
