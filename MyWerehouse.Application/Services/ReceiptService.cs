using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class ReceiptService : IReceiptService
	{
		private readonly IReceiptRepo _receiptRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementService _palletMovementService;
		private readonly IInventoryService _inventoryService;
		private readonly IValidator<CreatePalletReceiptDTO> _palletValidator;
		private readonly IValidator<ReceiptDTO> _receiptValidator;
		//private readonly IValidator<UpdatePalletDTO> _updateValidator;//chyba do wywalenia

		public ReceiptService(
			IReceiptRepo receiptRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IPalletMovementService palletMovementService,
			IInventoryService inventoryService,
			IValidator<CreatePalletReceiptDTO>? palletValidator,
			IValidator<ReceiptDTO>? receiptValidator			
			)
		{
			_receiptRepo = receiptRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_palletMovementService = palletMovementService;
			_inventoryService = inventoryService;
			_palletValidator = palletValidator;
			_receiptValidator = receiptValidator;			
		}		
		public async Task<int> CreateReceiptPlanAsync(CreateReceiptPlanDTO createReceiptPlanDTO)
		{
			var receipt = _mapper.Map<Receipt>(createReceiptPlanDTO);
			receipt.ReceiptDateTime = DateTime.UtcNow;
			receipt.ReceiptStatus = ReceiptStatus.Planned;

			await _receiptRepo.AddReceiptAsync(receipt);
			await _werehouseDbContext.SaveChangesAsync();
			return receipt.Id;
		}		
		public async Task<string> AddPalletToReceiptAsync(int receiptId, CreatePalletReceiptDTO newPalletDto)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.Planned && receipt.ReceiptStatus != ReceiptStatus.InProgress)
			{
				throw new InvalidOperationException("Nie można dodać palety zły status przyjęcia lub brak otworzenia przyjęcia");
			}
			var validationResult = _palletValidator.Validate(newPalletDto);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())//UOW do przeniesienia do kontrolera
			{
				try
				{
					if (receipt.ReceiptStatus == ReceiptStatus.Planned)
					{
						receipt.ReceiptStatus = ReceiptStatus.InProgress;
					}
					var pallet = _mapper.Map<Pallet>(newPalletDto);
					pallet.ReceiptId = receiptId;
					pallet.Id = await _palletRepo.GetNextPalletIdAsync();//kolejny numer palety
					pallet.LocationId = 1;//lokalizacja początkowa
					pallet.DateReceived = DateTime.Now;
					pallet.Status = PalletStatus.Receiving;
					await _palletRepo.AddPalletAsync(pallet);
					await _werehouseDbContext.SaveChangesAsync();
					await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Received, newPalletDto.UserId, PalletStatus.Receiving, null);
					await _werehouseDbContext.SaveChangesAsync();
					await transaction.CommitAsync();
					return pallet.Id;
				}
				catch (Exception)
				{
					await transaction.RollbackAsync();
					throw;
				}
			}
		}		
		public async Task CompletePhysicalReceiptAsync(int receiptId, string userId)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.InProgress)
			{
				throw new InvalidOperationException("Nie można zakończyć przyjęcia");
			}
			receipt.ReceiptStatus = ReceiptStatus.PhysicallyCompleted;
			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task VerifyAndFinalizeReceiptAsync(int receiptId, string userId)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				throw new InvalidOperationException("Nie można zweryfikować przyjęcia");
			}
			//logika zatwierdzenia dokumentów
			receipt.ReceiptStatus = ReceiptStatus.Verified;
			foreach (var pallet in receipt.Pallets)
			{
				pallet.Status = PalletStatus.InStock;
				await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Received, userId, PalletStatus.InStock, null);
				foreach (var product in pallet.ProductsOnPallet)
				{
					await _inventoryService.ChangeProductQunatityAsync(product.Id, product.Quantity);
				}				
			}
			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task<ReceiptDTO> GetReceiptDTOAsync(int receiptId)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			var receiptDTO = _mapper.Map<ReceiptDTO>(receipt);
			return receiptDTO;
		}		
		public async Task UpdateReceiptPalletsAsync(ReceiptDTO updatingReceipt, string userId)
		{
			var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(updatingReceipt.Id);
			var oldCollection = existingReceipt;
			if (existingReceipt == null)
			{
				throw new InvalidOperationException("Nie znaleziono przyjęcia");
			}
			var validationResult = _receiptValidator.Validate(updatingReceipt);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			//_mapper.Map(updatingReceipt, existingReceipt); //problem z Automaperem bo śledzi encję podległą
			//maper dodaje encję od pallet trzeba dodać wartości przyjęcia podstawowe ręcznie
			existingReceipt.ClientId = updatingReceipt.ClientId;
			existingReceipt.ReceiptStatus = updatingReceipt.ReceiptStatus;
			existingReceipt.PerformedBy = userId;
			existingReceipt.ReceiptDateTime = DateTime.UtcNow;
			foreach (var pallet in updatingReceipt.Pallets)
			{
				if (!string.IsNullOrEmpty(pallet.Id) && pallet.ReceiptId != updatingReceipt.Id && pallet.ReceiptId != null)
				{
					throw new InvalidDataException($"Paleta o numerze {pallet.Id} należy do innego przyjęcia o numerze {pallet.ReceiptId}");
				}
			}
			//Usuwanie z bazy danych niepotrzebnych pallet
			var incomingPlletsIds = updatingReceipt.Pallets
				.Select(p => p.Id)
				.Where(id => !string.IsNullOrEmpty(id))
				.ToHashSet();
			var palletToDelete = existingReceipt.Pallets
				.Where(p => !incomingPlletsIds.Contains(p.Id))
				.ToList();
			foreach (var pallet in palletToDelete)
			{
				await _palletRepo.DeletePalletAsync(pallet.Id);
			}
			//Nadanie numeru nowej palety
			var newPalletDtos = updatingReceipt.Pallets.Where(p => string.IsNullOrEmpty(p.Id)).ToList();
			var newPalletIdMap = new Dictionary<UpdatePalletDTO, string>();
			foreach (var newPalletDto in newPalletDtos)
			{
				var newId = await _palletRepo.GetNextPalletIdAsync();
				newPalletIdMap[newPalletDto] = newId;
				newPalletDto.Id = newId;
			}
			//zebranie palet do stworzenia historii
			var palletsToRegisterMovement = new List<Pallet>();
			//Faktyczny update
			CollectionSynchronizer.SynchronizeCollection(
				existingReceipt.Pallets,
				updatingReceipt.Pallets,
				pallet => pallet.Id,
				dto => dto.Id,
				newPalletDto =>
				{
					var newPallet = _mapper.Map<Pallet>(newPalletDto);
					newPallet.LocationId = 1;
					newPallet.DateReceived = DateTime.UtcNow;
					newPallet.Status = PalletStatus.Receiving;
					newPallet.ReceiptId = existingReceipt.Id;
					newPallet.ProductsOnPallet = newPalletDto.ProductsOnPallet
					  .Select(p => _mapper.Map<ProductOnPallet>(p)).ToList();
					palletsToRegisterMovement.Add(newPallet);
					return newPallet;
				},
				(updatingPalletDto, existingPallet) =>
				{
					_mapper.Map(updatingPalletDto, existingPallet);
					SynchronizeProducts(existingPallet, updatingPalletDto.ProductsOnPallet);
					palletsToRegisterMovement.Add(existingPallet);
				});
			//tworzenie historii palety
			foreach (var pallet in palletsToRegisterMovement)
			{
				await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Correction, userId, PalletStatus.Receiving, null);
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		private void SynchronizeProducts(Pallet pallet, IEnumerable<ProductOnPalletDTO> productDto)
		{
			CollectionSynchronizer.SynchronizeCollection(
				pallet.ProductsOnPallet,
				productDto,
				product => product.Id,
				dto => dto.Id,
				dto => _mapper.Map<ProductOnPallet>(dto),
			(dto, entity) => _mapper.Map(dto, entity)
				);
		}
		public async Task DeleteReceiptAsync(int receiptId)
		{
			var receiptToDelete = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			if (receiptToDelete == null)
			{
				throw new InvalidDataException($"Brak przyjęcia o numerze{receiptId}");
			}
			if (receiptToDelete.ReceiptStatus == ReceiptStatus.Verified)
			{
				throw new InvalidDataException("Nie można usunąć zweryfikowanego przyjęcia");
			}
			await _receiptRepo.DeleteReceiptAsync(receiptId);
		}
	}
}
