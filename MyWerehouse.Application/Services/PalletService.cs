using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Exceptions;
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

		private readonly ILocationRepo _locationRepo;

		private readonly IMapper _mapper;

		private readonly IValidator<UpdatePalletDTO> _updateValidator;

		private readonly WerehouseDbContext _werehouseDbContext;

		public PalletService(
			IPalletRepo palletRepo,
			IHistoryService historyService,
			ILocationService locationService,
			IPalletMovementRepo palletMovementRepo,
			IPickingPalletRepo pickingPalletRepo,
			ILocationRepo locationRepo,
			IMapper mapper,
			IValidator<UpdatePalletDTO> updateValidator,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_historyService = historyService;
			_locationService = locationService;
			_palletMovementRepo = palletMovementRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_locationRepo = locationRepo;
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
		public async Task<string> CreatePalletAsync(PalletDTO addPalletDTO, string userId)
		{
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			//pallet.Status = PalletStatus.InStock;
			_palletRepo.AddPallet(pallet);

			_historyService.CreateOperation(pallet, userId, PalletStatus.Available);
			await _werehouseDbContext.SaveChangesAsync();
			return pallet.Id;			
		}
		public async Task DeletePalletAsync(string id) //chyba tylko dla receipt ale tam na razie nie używam
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id)
				?? throw new PalletNotFoundException($"Nie ma palety o numerze {id}");
			var canDelete = await _palletMovementRepo.CanDeletePalletAsync(id);
			if (!canDelete)
				throw new PalletNotFoundException($"Palety o numerze {id} nie można usunąć");

			_palletRepo.DeletePallet(pallet);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<UpdatePalletDTO> GetPalletToEditAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id);
			var palletDTO = _mapper.Map<UpdatePalletDTO>(pallet);
			return palletDTO;
		}
		public async Task UpdatePalletAsync(UpdatePalletDTO updatingPallet, string userId)
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
			_historyService.CreateOperation(existingPallet, userId, existingPallet.Status);
			//Inventory?
			await _werehouseDbContext.SaveChangesAsync();
		}
		//public async Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id)
		//{
		//	var pallet = await _palletRepo.GetPalletByIdAsync(id)
		//	?? throw new PalletNotFoundException($"Paleta o numerze {id} nie istnieje");
		//	if (_werehouseDbContext == null)
		//		throw new Exception("DbContext is NULL");
		//	if (id == null)
		//		throw new Exception("ID is NULL");
		//	//var palletPalletMovements = await _historyService.GetHistoryPalletByIdAsync(id);
		//	var palletPalletMovements = await _werehouseDbContext.PalletMovements.Where(p => p.PalletId == id).ToListAsync();
		//	var palletHistory = _mapper.Map<PalletHistoryDTO>(pallet);
		//	palletHistory.PalletMovementsDTO = new List<PalletMovementDTO>();
		//	foreach (var movement in palletPalletMovements)
		//	{
		//		var movementDatailsDTO = new List<PalletMovementDetailDTO>();
		//		foreach (var item in movement.PalletMovementDetails)
		//		{
		//			var newMovementDetail = new PalletMovementDetailDTO
		//			{
		//				ProductId = item.ProductId,
		//				QuantityChange = item.Quantity,						
		//			};
		//			movementDatailsDTO.Add(newMovementDetail);
		//		}
		//		var movementDTO = new PalletMovementDTO
		//		{
		//			PalletId = movement.PalletId,
		//			Reason = movement.Reason,
		//			MovementDate = movement.MovementDate,
		//			DestinationLocationId = movement.DestinationLocationId,
		//			SourceLocationId = movement.SourceLocationId,
		//			PerformedBy = movement.PerformedBy,
		//			PalletMovementDetailsDTO = movementDatailsDTO
					
		//		};
		//		palletHistory.PalletMovementsDTO.Add(movementDTO);
		//	}
		//	return palletHistory;
		//}
		public async Task<ChangeLocationResults> ChangeLocationPalletAsync(string palletId, int destinationLocationId, string userId, bool force = false)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(palletId) ?? throw new PalletNotFoundException(palletId);
				//sprawdzenie czy lokalizacja jest zajęta
				//tu front musi przy pomocy backanedu wyliczyć locationId
				if (destinationLocationId <= 0)
					return new ChangeLocationResults
					{
						Success = false,
						Message = "niprawidłowa lokalizacja."
					};
				var existingPalletInDestination = await _palletRepo.CheckOccupancyAsync(destinationLocationId); // Nowa metoda repo
				var locationDTO = await _locationService.GetLocationServiceAsync(destinationLocationId);
				var fullNameLocation = $" Bay = {locationDTO.Bay} Aisle = {locationDTO.Aisle} Position = {locationDTO.Position} Height ={locationDTO.Height}";
				if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id && !force) // Jeśli lokalizacja jest zajęta przez inną paletę
				{
					return new ChangeLocationResults
					{
						Success = false,
						RequiresConfirmation = true,
						Message = $"Lokalizacja {fullNameLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}."
						//OccupiedByPalletId = existingPalletInDestination.Id // Opcjonalnie: Dodaj pole do Results (frontend pokaże)
					};
				}
				var destinationLocation = await _locationRepo.GetLocationByIdAsync(destinationLocationId);
				_historyService.CreateMovement(pallet, destinationLocation, ReasonMovement.Moved, userId, pallet.Status, null);
				pallet.LocationId = destinationLocationId;
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				return new ChangeLocationResults
				{
					Success = true,
					RequiresConfirmation = false,
					Message = $"Paleta {pallet.Id} została umieszczona w lokalizacji. "
				};
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
		public async Task<List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter)
		{
			var pallet = _palletRepo.GetPalletsByFilter(filter) ?? throw new PalletNotFoundException("Brak palety/palet o zadanych parametrach");
			var palletDTO = await pallet.ProjectTo<PalletDTO>(_mapper.ConfigurationProvider).ToListAsync();
			return palletDTO;
		}
		public async Task<VirtualPallet> AddPalletToPickingAsync(Issue issue, int productId, DateOnly? bestBefore, string userId)
		{
			var newPalletsToPicking = await _palletRepo.GetAvailablePallets(productId, bestBefore).ToListAsync();
			var newPallet = newPalletsToPicking.Where(a => a.Status == PalletStatus.Available ||
			a.Status == PalletStatus.InStock || a.Status == PalletStatus.Receiving).First() ?? throw new PalletNotFoundException("Brak palet do pickingu");
			var newVirtualPicking = new VirtualPallet
			{
				Pallet = newPallet,
				PalletId = newPallet.Id,
				DateMoved = DateTime.UtcNow,
				LocationId = newPallet.LocationId,
				IssueInitialQuantity = newPallet.ProductsOnPallet.First(p => p.PalletId == newPallet.Id).Quantity,//zakładam że jest jeden towar
				Allocations = new List<Allocation>()
			};
			var virtualPallet = _pickingPalletRepo.AddPalletToPicking(newVirtualPicking);
			_palletRepo.ChangePalletStatus(newPallet.Id, PalletStatus.ToPicking); //zmiana statusu dla palety
			_historyService.CreateOperation(newPallet, newPallet.LocationId, ReasonMovement.Picking, userId, PalletStatus.ToPicking, null);
			return virtualPallet;
		}
		public async Task<List<Pallet>> GetAllAvailablePalletsAsync(int productId, DateOnly? bestBefore)
		{
			var tracked = _werehouseDbContext.ChangeTracker.Entries<Pallet>()
			.Where(e => e.Entity.Status == PalletStatus.Available
					&& e.Entity.ProductsOnPallet.Any(prod => prod.ProductId == productId
					&& (bestBefore == null || prod.BestBefore == bestBefore)))
			.Select(e => e.Entity)
			.ToList();
			var trackedIds = tracked.Select(p => p.Id).ToHashSet();

			var fromDb = await _palletRepo.GetAvailablePallets(productId, bestBefore)
				.Where(p => !trackedIds.Contains(p.Id))
				.ToListAsync();

			return tracked.Concat(fromDb).ToList();
		}

		
	}
}
