using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptHandler : IRequestHandler<UpdateReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IClientRepo _clientRepo;
		private readonly ILocationRepo _locationRepo;		

		public UpdateReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IPublisher mediator,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IClientRepo clientRepo,
			ILocationRepo locationRepo)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_clientRepo = clientRepo;
			_locationRepo = locationRepo;		
		}
		public async Task<AppResult<Unit>> Handle(UpdateReceiptCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				// Palety nie wpływają na stan magazynu do momentu zatwierdzenia przyjęcia
				// Pallets don't change warehouse's stock until receipt is confirmed
				var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(request.DTO.ReceiptId); 
				if (existingReceipt == null)
					return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.DTO.ReceiptNumber} nie zostało znalezione.", ErrorType.NotFound);
				if (!await _clientRepo.IsClientExistAsync(request.DTO.ClientId))
				{
					return AppResult<Unit>.Fail("Wybrany klient nie istnieje.", ErrorType.NotFound);
				}
				if (!await _locationRepo.ReceivingRampExistsAsync(request.DTO.RampNumber))
					return AppResult<Unit>.Fail("Wybrana rampa nie istnieje.", ErrorType.NotFound);
				foreach (var item in request.DTO.Pallets)
				{
					if (item.ReceiptId != null && item.ReceiptId != existingReceipt.Id)
					{
						return AppResult<Unit>.Fail($"Paleta o numerze {item.PalletNumber} należy do innego przyjęcia.", ErrorType.Conflict);
					}
				}
				//List palet do usunięcia z bazy danych 
				var incomingPalletsIds = request.DTO.Pallets
					.Select(p => p.Id)
					.Where(id => id != Guid.Empty)//
					.ToHashSet();
				var palletToDelete = existingReceipt.Pallets
					.Where(p => !incomingPalletsIds.Contains(p.Id))
					.ToList();
				//Usuwanie z bazy danych niepotrzebnych pallet
				foreach (var pallet in palletToDelete)
				{
					existingReceipt.DetachPallet(pallet);//musi być żeby stworzyć dobrą historię					
					pallet.DetachFromReceipt(request.UserId, pallet.Location.ToSnopShot());
				}
				var existingPallets = existingReceipt.Pallets.ToDictionary(p => p.Id);
				//Aktualizacja palet
				foreach (var dto in request.DTO.Pallets.Where(p => p.Id != null))
				{
					if (!existingPallets.TryGetValue(dto.Id!, out var pallet))
						continue;

					var productsForPallet = new List<ProductOnPallet>();

					foreach (var product in dto.ProductsOnPallet) //możliwość na kilka produktów na razie zbędne
					{
						var productForPallet = ProductOnPallet.Create(product.ProductId,
							product.PalletId, product.Quantity, product.DateAdded, product.BestBefore);						
						productsForPallet.Add(productForPallet);
					}

					pallet.UpdateProductChanges(productsForPallet);
					pallet.ChangeStatus(PalletStatus.Receiving);
					pallet.AddHistory(ReasonMovement.Correction, request.UserId, pallet.Location.ToSnopShot());
				}
				//Dodanie nowych palet
				var palletsAdded = request.DTO.Pallets
					.Where(p => p.Id == Guid.Empty)//
					.ToList();
				foreach (var palletToAdd in palletsAdded)
				{
					var newId = await _palletRepo.GetNextPalletIdAsync();
					var location = await _locationRepo.GetLocationByIdAsync(request.DTO.RampNumber);
						if (location == null) return AppResult<Unit>.Fail($"Lokalizacja o numerze {request.DTO.RampNumber} nie została znaleziona", ErrorType.NotFound);
					var pallet = Pallet.Create(newId, request.DTO.RampNumber);
					foreach (var dto in palletToAdd.ProductsOnPallet)
					{
						if (!await _productRepo.IsExistProduct(dto.ProductId))
							return AppResult<Unit>.Fail($"Produkt o numerze {dto.ProductId} nie istnieje.", ErrorType.NotFound); 
						pallet.AddProduct(dto.ProductId, dto.Quantity, dto.BestBefore);
					}
					var snapShot = location.ToSnopShot();
					_palletRepo.AddPallet(pallet);

					pallet.AssignToReceipt(existingReceipt.Id, snapShot, request.UserId);
					existingReceipt.AttachPallet(pallet);
				}
				existingReceipt.UpdateReceipt(request.UserId, request.DTO.ClientId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, $"Przyjęcie o numerze {existingReceipt.ReceiptNumber} zostało zaktualizowane");
			}
			
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return AppResult<Unit>.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}
}
