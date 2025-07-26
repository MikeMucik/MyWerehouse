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
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class PalletService : IPalletService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		//private readonly IProductOnPalletRepo _productOnPalletRepo;
		private readonly IMapper _mapper;
		private readonly IValidator<CreatePalletPickingDTO> _pickingValidator;
		private readonly IValidator<UpdatePalletDTO> _updateValidator;
		private readonly WerehouseDbContext _werehouseDbContext;

		public PalletService(
			IPalletRepo palletRepo,
			IPalletMovementRepo? palletMovementRepo,
			//IProductOnPalletRepo productOnPalletRepo,
			IMapper mapper,
			IValidator<CreatePalletPickingDTO>? pickingValidator,
			IValidator<UpdatePalletDTO>? updateValidator,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			//_productOnPalletRepo = productOnPalletRepo;
			_mapper = mapper;
			_pickingValidator = pickingValidator;
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
		public async Task<string> CreatePickingPalletAsync(CreatePalletPickingDTO addPalletDTO)
		{
			addPalletDTO.Id = await _palletRepo.GetNextPalletIdAsync();
			addPalletDTO.DateCreated = DateTime.Now;
			var validationResult = await _pickingValidator.ValidateAsync(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = await _palletRepo.AddPalletAsync(pallet);

			var listRecordsPMD = new List<PalletMovementDetail>();
			foreach (var product in pallet.ProductsOnPallet)
			{
				var recordPalletMovementDetails = new PalletMovementDetail
				{
					ProductId = product.ProductId,
					Quantity = product.Quantity,
				};
				listRecordsPMD.Add(recordPalletMovementDetails);
			}
			var recordPM = new PalletMovement
			{
				PalletId = id,
				DestinationLocationId = pallet.LocationId,
				Reason = ReasonMovement.Picking,
				PerformedBy = addPalletDTO.UserId,
				MovementDate = DateTime.Now,
				PalletMovementDetails = listRecordsPMD
			};
			await _palletMovementRepo.AddPalletMovementAsync(recordPM);
			await _werehouseDbContext.SaveChangesAsync();
			return id;
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
			//var a = existingPallet.ProductsOnPallet.Count();
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


			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id)
			?? throw new ArgumentNullException($"Paleta o numerze {id} nie istnieje");
			var palletHistory = _mapper.Map<PalletHistoryDTO>(pallet);
			return palletHistory;
		}
		public async Task ChangeLocationPalletAsync(string palletId, int destinationLocation, string userId)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			if (pallet == null)
			{
				throw new ArgumentNullException($"Paleta o numerze {palletId} nie została znaleziona");
			}
			//sprawdzenie czy lokalizacja jest zajęta
			var existingPalletInDestination = await _palletRepo.GetPalletByLocationAsync(destinationLocation); // Nowa metoda repo

			if (existingPalletInDestination != null && existingPalletInDestination.Id != pallet.Id) // Jeśli lokalizacja jest zajęta przez inną paletę
			{
				throw new InvalidOperationException($"Lokalizacja {destinationLocation} jest już zajęta przez paletę {existingPalletInDestination.Id}.");
			}
			var sourceLocation = pallet.LocationId;
			pallet.LocationId = destinationLocation;
			var listRecordsPMD = new List<PalletMovementDetail>();
			foreach (var product in pallet.ProductsOnPallet)
			{
				var recordPalletMovementDetails = new PalletMovementDetail
				{
					ProductId = product.ProductId,
					Quantity = product.Quantity,
				};
				listRecordsPMD.Add(recordPalletMovementDetails);
			}
			var recordPM = new PalletMovement
			{
				PalletId = palletId,
				SourceLocationId = sourceLocation,
				DestinationLocationId = destinationLocation,
				Reason = ReasonMovement.ManualMove,
				PerformedBy = userId,
				MovementDate = DateTime.Now,
				PalletMovementDetails = listRecordsPMD
			};
			await _palletMovementRepo.AddPalletMovementAsync(recordPM);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//TODO
		public async Task<List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter)
		{
			var pallet = _palletRepo.GetPalletsByFilter(filter) ?? throw new ArgumentException("Brak palety/palet o zadanych parametrach");
			var palletDTO = await pallet.ProjectTo<PalletDTO>(_mapper.ConfigurationProvider).ToListAsync();
			return palletDTO;
		}
	}
}
