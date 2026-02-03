using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.Commands.DeletePallet;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Domain.Pallets.Filters;

namespace MyWerehouse.Application.Services
{
	public class PalletService : IPalletService
	{
		private readonly IMediator _mediator;
		public PalletService(
			IMediator mediator)
		{
			_mediator = mediator;
		}
		public async Task<PalletResult> CreatePalletAsync(PalletDTO addPalletDTO, string userId)
		{
			return await _mediator.Send(new CreateNewPalletCommand(addPalletDTO, userId));				
		}
		public async Task<PalletResult> DeletePalletAsync(string id, string userId) //chyba tylko dla receipt ale tam na razie nie używam
		{
			return await _mediator.Send(new DeletePalletCommand(id, userId));			
		}
		public async Task<UpdatePalletDTO> GetPalletToEditAsync(string id)
		{
			return await _mediator.Send(new GetPalletToEditQuery(id));			
		}
		public async Task<PalletResult> UpdatePalletAsync(UpdatePalletDTO updatingPallet, string userId)
		{
			return await _mediator.Send(new UpdatePalletCommand(updatingPallet, userId));
			
		}

		public async Task<ChangeLocationResults> ChangeLocationPalletAsync(string palletId, int destinationLocationId, string userId, bool force = false)
		{
			return await _mediator.Send(new ChangeLocationPalletCommand(palletId, destinationLocationId, userId, force));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var pallet = await _palletRepo.GetPalletByIdAsync(palletId) ?? throw new PalletException(palletId);
			//	//sprawdzenie czy lokalizacja jest zajęta
			//	//tu front musi przy pomocy backanedu wyliczyć locationId
			//	if (destinationLocationId <= 0)
			//		return new ChangeLocationResults
			//		{
			//			Success = false,
			//			Message = "niprawidłowa lokalizacja."
			//		};
			//	var existingPalletInDestination = await _palletRepo.CheckOccupancyAsync(destinationLocationId); // Nowa metoda repo
			//	var locationDTO = await _locationService.GetLocationServiceAsync(destinationLocationId);
			//	var fullNameLocation = $" Bay = {locationDTO.Bay} Aisle = {locationDTO.Aisle} Position = {locationDTO.Position} Height ={locationDTO.Height}";
			//	if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id && !force) // Jeśli lokalizacja jest zajęta przez inną paletę
			//	{
			//		return new ChangeLocationResults
			//		{
			//			Success = false,
			//			RequiresConfirmation = true,
			//			Message = $"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}."
			//			//OccupiedByPalletId = existingPalletInDestination.Id // Opcjonalnie: Dodaj pole do Results (frontend pokaże)
			//		};
			//	}
			//	var destinationLocation = await _locationRepo.GetLocationByIdAsync(destinationLocationId);
			//	//_historyService.CreateMovement(pallet, destinationLocation, ReasonMovement.Moved, userId, pallet.Status, null);
			//	await _mediator.Publish(new CreatePalletMovementNotification(
			//		pallet.Id,
			//		pallet.LocationId,
			//		destinationLocationId,
			//		ReasonMovement.Moved,
			//		userId,
			//		pallet.Status,
			//		null));
			//	pallet.LocationId = destinationLocationId;
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	return new ChangeLocationResults
			//	{
			//		Success = true,
			//		RequiresConfirmation = false,
			//		Message = $"Paleta {pallet.Id} została umieszczona w lokalizacji. "
			//	};
			//}
			//catch (Exception)
			//{
			//	await transaction.RollbackAsync();
			//	throw;
			//}
		}
		public async Task<List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter)
		{
			return await _mediator.Send(new FindPalletsByFiltrQuery(filter));			
		}		
	}
}
