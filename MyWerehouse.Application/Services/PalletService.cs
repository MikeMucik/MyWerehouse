using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Services
{
	public class PalletService : IPalletService
	{
		private readonly IHistoryService _historyService;
		private readonly ILocationService _locationService;

		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;

		private readonly IMapper _mapper;

		private readonly IValidator<UpdatePalletDTO> _updateValidator;

		private readonly WerehouseDbContext _werehouseDbContext;

		public PalletService(
			IPalletRepo palletRepo,
			IHistoryService historyService,
			ILocationService locationService,
			IPalletMovementRepo palletMovementRepo,
			IPickingPalletRepo pickingPalletRepo,
			IMapper mapper,
			IValidator<UpdatePalletDTO> updateValidator,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_historyService = historyService;
			_locationService = locationService;
			_palletMovementRepo = palletMovementRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_mapper = mapper;
			_updateValidator = updateValidator;
			_werehouseDbContext = werehouseDbContext;
		}
		public PalletService(
			IPalletRepo palletRepo,
			IMapper mapper)
		{
			_palletRepo = palletRepo;
			_mapper = mapper;
		}

		public async Task DeletePalletAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id);
			if (pallet == null)
				throw new ArgumentException($"Nie ma palety o numerze {id}");

			var canDelete = await _palletMovementRepo.CanDeletePalletAsync(id);
			if (!canDelete)
				throw new InvalidOperationException($"Palety o numerze {id} nie można usunąć");

			await _palletRepo.DeletePalletAsync(id);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<UpdatePalletDTO> GetPalletToEditAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id);
			var palletDTO = _mapper.Map<UpdatePalletDTO>(pallet);
			return palletDTO;
		}
		public async Task UpdatePalletAsync(UpdatePalletDTO updatingPallet)
		{
			var existingPallet = await _palletRepo.GetPalletByIdAsync(updatingPallet.Id);
			var validationResult = await _updateValidator.ValidateAsync(updatingPallet);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			_mapper.Map(updatingPallet, existingPallet);
			CollectionSynchronizer.SynchronizeCollection(
				existingPallet.ProductsOnPallet,
				updatingPallet.ProductsOnPallet,
				a => a.Id,
				a => a.Id,
				dto =>
				{
					var newProduct = _mapper.Map<ProductOnPallet>(dto);
					newProduct.PalletId = existingPallet.Id;
					return newProduct;
				},
				(dto, entity) => _mapper.Map(dto, entity));

			//PalletMovement ?
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id)
			?? throw new ArgumentNullException($"Paleta o numerze {id} nie istnieje");
			var palletHistory = _mapper.Map<PalletHistoryDTO>(pallet);
			return palletHistory;
		}
		public async Task<ChangeLocationResults> ChangeLocationPalletAsync(string palletId, int destinationLocation, string userId, bool force = false)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			if (pallet == null)
			{
				throw new ArgumentNullException($"Paleta o numerze {palletId} nie została znaleziona");
			}
			//sprawdzenie czy lokalizacja jest zajęta
			var existingPalletInDestination = await _palletRepo.GetPalletByLocationAsync(destinationLocation); // Nowa metoda repo
			var locationDTO = await _locationService.GetLocationServiceAsync(destinationLocation);
			var fullNameLocation = $" Bay = {locationDTO.Bay} Aisle = {locationDTO.Aisle} Position = {locationDTO.Position} Height ={locationDTO.Height}";
			if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id && !force) // Jeśli lokalizacja jest zajęta przez inną paletę
			{
				return new ChangeLocationResults
				{
					Success = false,
					RequiresConfirmation = true,
					Message = $"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}."
				};
			}
			pallet.LocationId = destinationLocation;
			await _historyService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Moved, userId, pallet.Status, null);
			await _werehouseDbContext.SaveChangesAsync();
			return new ChangeLocationResults
			{
				Success = true,
				RequiresConfirmation = false,
				Message = $"Paleta {pallet.Id} została umieszczona w lokalizacji. "
			};
		}
		//TODO
		public async Task<List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter)
		{
			var pallet = _palletRepo.GetPalletsByFilter(filter) ?? throw new ArgumentException("Brak palety/palet o zadanych parametrach");
			var palletDTO = await pallet.ProjectTo<PalletDTO>(_mapper.ConfigurationProvider).ToListAsync();
			return palletDTO;
		}

		public async Task<int> AddPalletToPickingAsync(int issueId, int productId, DateOnly? bestBefore, string userId)
		{
			var newPalletsToPicking = await _palletRepo.GetAvailablePallets(productId, bestBefore).ToListAsync();
			var newPallet = newPalletsToPicking.First() ?? throw new InvalidOperationException("Brak palet do pickingu");
			var virtualPalletId = await _pickingPalletRepo.AddPalletToPickingAsync(newPallet.Id);
			await _palletRepo.ChangePalletStatusAsync(newPallet.Id, PalletStatus.ToPicking); //zmiana statusu dla palety
			await _historyService.CreateMovementAsync(newPallet, newPallet.LocationId, ReasonMovement.Picking, userId, PalletStatus.ToPicking, null);
			await _werehouseDbContext.SaveChangesAsync();//by wykluczyć wzięcie palety do Issue i dodać paletę do pickingu	
			return virtualPalletId;
		}
	}
}
