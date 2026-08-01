using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;
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
		private readonly ILocationRepo _locationRepo;
		private readonly IDateTimeProvider _dateTimeProvider;
		public UpdateReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			ILocationRepo locationRepo,
			IDateTimeProvider dateTimeProvider)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_locationRepo = locationRepo;
			_dateTimeProvider = dateTimeProvider;
		}
		public async Task<AppResult<Unit>> Handle(UpdateReceiptCommand request, CancellationToken ct)
		{
			// Palety nie wpływają na stan magazynu do momentu zatwierdzenia przyjęcia	
			var now = _dateTimeProvider.UtcNow;
			var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(request.Id);
			if (existingReceipt == null)
				return AppResult<Unit>.Fail($"Receipt was not found.");
			foreach (var item in request.DTO.Pallets)
			{
				if (item.ReceiptId != null && item.ReceiptId != existingReceipt.Id)
				{
					return AppResult<Unit>.Fail($"Pallet {item.PalletNumber} belongs to another receipt.", ErrorType.Conflict);
				}
			}
			//List palet do usunięcia z bazy danych 
			var incomingPalletsIds = request.DTO.Pallets
				.Select(p => p.Id)
				.Where(id => id != Guid.Empty)
				.ToHashSet();
			var palletToDelete = existingReceipt.Pallets
				.Where(p => !incomingPalletsIds.Contains(p.Id))
				.ToList();
			//Usuwanie z bazy danych niepotrzebnych pallet
			foreach (var pallet in palletToDelete)
			{
				existingReceipt.DetachPallet(pallet);//musi być żeby stworzyć dobrą historię					
				pallet.DetachFromReceipt(request.DTO.PerformedBy, pallet.Location.ToSnapshot());
			}
			var existingPallets = existingReceipt.Pallets.ToDictionary(p => p.Id);
			//Aktualizacja palet
			foreach (var dto in request.DTO.Pallets.Where(p => p.Id != Guid.Empty))
			{
				if (!existingPallets.TryGetValue(dto.Id!, out var pallet))
					continue;

				var productsForPallet = new List<ProductOnPallet>();

				if (dto.ProductsOnPallet.Count != 1)
				{
					return AppResult<Unit>.Fail($"A receiving pallet can contain only one product.", ErrorType.Conflict);
				}
				var product = dto.ProductsOnPallet.Single();

				if (!await _productRepo.IsExistProduct(product.ProductId))
					return AppResult<Unit>.Fail($"Product {product.ProductId} does not exist.");

				var productForPallet = ProductOnPallet.Create(product.ProductId,
					product.PalletId, product.Quantity, product.DateAdded, product.BestBefore);

				productsForPallet.Add(productForPallet);
				
				pallet.ReplaceProducts(productsForPallet);
				pallet.ChangeStatus(PalletStatus.Receiving);
				pallet.AddHistory(ReasonForPallet.Correction, request.DTO.PerformedBy, pallet.Location.ToSnapshot());
			}
			//Dodanie nowych palet - Adding new palets
			var palletsAdded = request.DTO.Pallets
				.Where(p => p.Id == Guid.Empty)
				.ToList();
			foreach (var palletToAdd in palletsAdded)
			{
				var newId = await _palletRepo.GetNextPalletIdAsync();
				var location = await _locationRepo.GetLocationByIdAsync(request.DTO.RampNumber);
				if (location == null)
				{
					return AppResult<Unit>.Fail("The specified location is invalid.", ErrorType.Validation);
				}
				
				var pallet = Pallet.Create(newId, request.DTO.RampNumber, now);
				foreach (var dto in palletToAdd.ProductsOnPallet)
				{
					if (!await _productRepo.IsExistProduct(dto.ProductId))
						return AppResult<Unit>.Fail($"Product {dto.ProductId} does not exist.");
					pallet.AddProduct(dto.ProductId, dto.Quantity, now, dto.BestBefore);
				}
				var snapShot = location.ToSnapshot();
				_palletRepo.AddPallet(pallet);
				pallet.AssignToReceipt(existingReceipt.Id, snapShot, request.DTO.PerformedBy);
				existingReceipt.AttachPallet(pallet);
			}
			existingReceipt.UpdateReceipt(request.DTO.PerformedBy, request.DTO.ClientId, now);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Receipt {existingReceipt.ReceiptNumber} was updated.");
		}
	}
}
