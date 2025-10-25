using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.HistoryDTO;
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
		private readonly IHistoryService _historyService;
		private readonly IInventoryService _inventoryService;
		private readonly ILocationRepo _locationRepo;
		private readonly IValidator<CreatePalletReceiptDTO> _palletValidator;
		private readonly IValidator<ReceiptDTO> _receiptValidator;

		public ReceiptService(
			IReceiptRepo receiptRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IHistoryService historyService,
			IInventoryService inventoryService,
			ILocationRepo locationRepo,
			IValidator<CreatePalletReceiptDTO>? palletValidator,
			IValidator<ReceiptDTO>? receiptValidator
			)
		{
			_receiptRepo = receiptRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_historyService = historyService;
			_inventoryService = inventoryService;
			_locationRepo = locationRepo;
			_palletValidator = palletValidator;
			_receiptValidator = receiptValidator;
		}		
		public ReceiptService(
			IReceiptRepo receiptRepo,
			IMapper mapper)
		{
			_receiptRepo = receiptRepo;
			_mapper = mapper;
		}
		public async Task<ReceiptResult> CreateReceiptPlanAsync(CreateReceiptPlanDTO createReceiptPlanDTO)
		{
			try
			{
				var receipt = _mapper.Map<Receipt>(createReceiptPlanDTO);
				receipt.ReceiptDateTime = DateTime.UtcNow;
				receipt.ReceiptStatus = ReceiptStatus.Planned;
				_historyService.CreateHistoryReceipt(receipt);
				_receiptRepo.AddReceipt(receipt);
				await _werehouseDbContext.SaveChangesAsync();
				return ReceiptResult.Ok("Utworzono przyjęcie", receipt.Id);
			}
			catch (Exception ex)
			{				
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}	
		public async Task<ReceiptResult> AddPalletToReceiptAsync(int receiptId, CreatePalletReceiptDTO newPalletDto)
		{
			//var result = new ReceiptResult();
			var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			if (receipt == null || (receipt.ReceiptStatus != ReceiptStatus.Planned && receipt.ReceiptStatus != ReceiptStatus.InProgress))
			{
				throw new ReceiptNotFoundException("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
			}
			var validationResult = _palletValidator.Validate(newPalletDto);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())
			{
				try
				{
					if (receipt.ReceiptStatus == ReceiptStatus.Planned)
					{
						receipt.ReceiptStatus = ReceiptStatus.InProgress;
						receipt.ReceiptDateTime = DateTime.UtcNow;
						_historyService.CreateHistoryReceipt(receipt);
					}
					var pallet = _mapper.Map<Pallet>(newPalletDto);
					pallet.ReceiptId = receiptId;
					pallet.Id = await _palletRepo.GetNextPalletIdAsync();//kolejny numer palety
					pallet.LocationId = 1;//lokalizacja początkowa
					pallet.DateReceived = DateTime.UtcNow;
					pallet.Status = PalletStatus.Receiving;
					_palletRepo.AddPallet(pallet);
					_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Received, newPalletDto.UserId, PalletStatus.Receiving, null);
					await _werehouseDbContext.SaveChangesAsync();
					await transaction.CommitAsync();
					return ReceiptResult.Ok($"Paleta {pallet.Id} została dodana do przyjęcia {receiptId}", pallet.Id);
				}
				catch (ReceiptNotFoundException erp)
				{
					await transaction.RollbackAsync();
					return ReceiptResult.Fail(erp.Message);
				}
				catch (ValidationException ev)
				{
					await transaction.RollbackAsync();
					return ReceiptResult.Fail(ev.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
		public async Task<ReceiptResult> CompletePhysicalReceiptAsync(int receiptId, string userId)
		{
			try
			{
				var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
				if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.InProgress)
				{
					throw new ReceiptNotFoundException("Nie można zakończyć przyjęcia");
				}
				receipt.ReceiptStatus = ReceiptStatus.PhysicallyCompleted;
				_historyService.CreateHistoryReceipt(receipt);
				await _werehouseDbContext.SaveChangesAsync();
				return ReceiptResult.Ok("Zakończono fizyczne przyjęcie - gotowe do weryfikacji", receiptId);
			}
			catch (ReceiptNotFoundException erp)
			{
				return ReceiptResult.Fail(erp.Message);
			}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
		public async Task<ReceiptResult> VerifyAndFinalizeReceiptAsync(int receiptId, string userId)
		{
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())
			{
				try
				{
					var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
					if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
					{
						throw new ReceiptNotFoundException("Nie można zweryfikować przyjęcia");
					}
					//logika zatwierdzenia dokumentów
					receipt.ReceiptStatus = ReceiptStatus.Verified;
					receipt.ReceiptDateTime = DateTime.UtcNow;
					_historyService.CreateHistoryReceipt(receipt);
					foreach (var pallet in receipt.Pallets)
					{
						pallet.Status = PalletStatus.InStock;
						_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Received, userId, PalletStatus.InStock, null);
						foreach (var product in pallet.ProductsOnPallet)
						{
							await _inventoryService.ChangeProductQuantityAsync(product.Id, product.Quantity);
						}
					}
					await _werehouseDbContext.SaveChangesAsync();
					await transaction.CommitAsync();
					return ReceiptResult.Ok("Palety z przyjęcia zweryfikowano, gotowe do działania", receiptId);
				}
				catch (ReceiptNotFoundException erp)
				{
					await transaction.RollbackAsync();
					return ReceiptResult.Fail(erp.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
		public async Task<ReceiptDTO> GetReceiptDTOAsync(int receiptId)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			var receiptDTO = _mapper.Map<ReceiptDTO>(receipt);
			return receiptDTO;
		}
		public async Task<ReceiptResult> UpdateReceiptPalletsAsync(ReceiptDTO updatingReceipt, string userId)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			try
			{
				var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(updatingReceipt.Id);
				if (existingReceipt == null) throw new ReceiptNotFoundException("Nie znaleziono przyjęcia");

				var validationResult = _receiptValidator.Validate(updatingReceipt);
				if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

				existingReceipt.ClientId = updatingReceipt.ClientId;
				existingReceipt.ReceiptStatus = updatingReceipt.ReceiptStatus;
				existingReceipt.PerformedBy = userId;
				existingReceipt.ReceiptDateTime = DateTime.UtcNow;
				foreach (var item in updatingReceipt.Pallets)
				{
					if (!string.IsNullOrEmpty(item.Id) && item.ReceiptId != updatingReceipt.Id && item.ReceiptId != null)
					{
						throw new PalletNotFoundException($"Paleta o numerze {item.Id} należy do innego przyjęcia o numerze {item.ReceiptId}");
					}
				}
				var palletsRaw = new List<Pallet>();
				foreach (var item in updatingReceipt.Pallets)
				{
					var pallet = await _palletRepo.GetPalletByIdAsync(item.Id)
					?? throw new PalletNotFoundException($"Nie znaleziono palety o Id: {item.Id}");

					palletsRaw.Add(pallet);
				}
				//Usuwanie z bazy danych niepotrzebnych pallet
				var incomingPalletsIds = updatingReceipt.Pallets
					.Select(p => p.Id)
					.Where(id => !string.IsNullOrEmpty(id))
					.ToHashSet();
				var palletToDelete = existingReceipt.Pallets
					.Where(p => !incomingPalletsIds.Contains(p.Id))
					.ToList();
				foreach (var pallet in palletToDelete)//
				{
					_palletRepo.DeletePallet(pallet);
				}

				foreach (var pallet in palletsRaw)
				{
					var dto = updatingReceipt.Pallets.First(p => p.Id == pallet.Id);

					pallet.ReceiptId = existingReceipt.Id;
					pallet.Status = PalletStatus.Receiving;
					pallet.DateReceived = DateTime.UtcNow;

					SynchronizeProducts(pallet, dto.ProductsOnPallet);
				}
				foreach (var pallet in palletsRaw)
				{
					if (!existingReceipt.Pallets.Any(x => x.Id == pallet.Id))
						existingReceipt.Pallets.Add(pallet);
				}
				_historyService.CreateHistoryReceipt(existingReceipt, ReceiptStatus.Correction, userId);
				//tworzenie historii palety			
				foreach (var pallet in palletsRaw)
				{
					if (pallet.Location == null && pallet.LocationId > 0)
					{						
						Location locationFull =await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
						pallet.Location = locationFull;
					}
					_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Correction, userId, PalletStatus.Receiving, null);
				}
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				return ReceiptResult.Ok($"Przyjęcie o numerze {updatingReceipt.Id} zostało zaktualizowane", updatingReceipt.Id);
				
			}
			catch(ReceiptNotFoundException exr)
			{
				await transaction.RollbackAsync();
				return ReceiptResult.Fail(exr.Message);
			}
			catch (PalletNotFoundException expal)
			{
				await transaction.RollbackAsync();
				return ReceiptResult.Fail(
					expal.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
			
		}
		public async Task<ReceiptResult> CancelReceiptAsync(int receiptId, string userId)
		{
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())
			{
				try
				{
					var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
					if (receipt == null)
					{
						throw new ReceiptNotFoundException($"Brak przyjęcia o numerze{receiptId}");
					}
					if (receipt.ReceiptStatus == ReceiptStatus.Verified)
					{
						throw new ReceiptNotFoundException("Nie można usunąć zweryfikowanego przyjęcia");
					}
					if (!(receipt.ReceiptStatus == ReceiptStatus.Planned
						|| receipt.ReceiptStatus == ReceiptStatus.InProgress
						|| receipt.ReceiptStatus == ReceiptStatus.PhysicallyCompleted))
					{
						throw new ReceiptNotFoundException("Nieprawidłowy status przyjęcia");
					}
					receipt.ReceiptStatus = ReceiptStatus.Cancelled;
					receipt.PerformedBy = userId;
					receipt.ReceiptDateTime = DateTime.UtcNow;

					_historyService.CreateHistoryReceipt(receipt, ReceiptStatus.Cancelled, userId);
					//usuwanie palet które jeszcze nie weszły w "życie" magazynu
					foreach (var pallet in receipt.Pallets.ToList())
					{
						//dodaj sprawdzenie czy można usunąć paletę??
						_palletRepo.DeletePallet(pallet);
					}
					//usunięcie całkowite przyjęcia- zostawienie tylko snapshotów
					await _werehouseDbContext.SaveChangesAsync();
					await transaction.CommitAsync();
					return ReceiptResult.Ok("Usunięto przyjęcie", receiptId);
				}
				catch (ReceiptNotFoundException erp)
				{
					await transaction.RollbackAsync();
					return ReceiptResult.Fail(erp.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
		private void SynchronizeProducts(Pallet pallet, IEnumerable<ProductOnPalletDTO> productDto)
		{
			foreach (var dto in productDto)
			{
				var existing = pallet.ProductsOnPallet
					.FirstOrDefault(p => p.ProductId == dto.ProductId && p.PalletId == dto.PalletId);

				if (existing != null)
				{
					dto.Id = existing.Id; // przypisz faktyczne Id
				}
			}
			CollectionSynchronizer.SynchronizeCollection(
				pallet.ProductsOnPallet,
				productDto,
				product => new { product.Id },
				//,product.ProductId, product.PalletId },
				dto => new { dto.Id },
				//, dto.ProductId, dto.PalletId },
				dto => _mapper.Map<ProductOnPallet>(dto),
			(dto, entity) => _mapper.Map(dto, entity)
				);
		}
	}
}
