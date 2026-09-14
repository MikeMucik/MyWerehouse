using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptHandler(IUnitOfWork unitOfWork,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		ILocationRepo locationRepo,
		IDateTimeProvider dateTimeProvider,
		IPalletNumberAllocator palletNumberAllocator) : IRequestHandler<UpdateReceiptCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		private readonly IPalletNumberAllocator _palletNumberAllocator = palletNumberAllocator;

		public async Task<AppResult<Unit>> Handle(UpdateReceiptCommand request, CancellationToken ct)
		{
			// Palety nie wpływają na stan magazynu do momentu zatwierdzenia przyjęcia
			var now = _dateTimeProvider.UtcNow;
			var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(request.Id, ct);
			if (existingReceipt == null)
				return AppResult<Unit>.Fail($"Receipt was not found.");
			//Sprawdzenie czy wszystkie rodzaje towaru istnieją w bazie
			var listProducts = request.DTO.Pallets
				.Select(a => a.ProductsOnPallet.Single().ProductId)//paleta przyjmowana ma tylko jeden produkt
				.Distinct()
				.ToList();
			foreach (var item in listProducts)
			{
				if (!await _productRepo.IsExistProduct(item, ct))
					return AppResult<Unit>.Fail($"Product {item} does not exist.");
			}
			var location = await _locationRepo.GetLocationByIdAsync(request.DTO.RampNumber, ct);
			if (location == null)
			{
				return AppResult<Unit>.Fail($"Location not exists.");
			}
			var snapShot = location.ToSnapshot();
			var palletsToUpdate = new List<ReceiptPalletUpdate>();
			foreach (var pallet in request.DTO.Pallets)
			{
				var palletTT = new ReceiptPalletUpdate
				(
					PalletId: pallet.Id,
					PalletNumber: pallet.PalletNumber,
					ProductId: pallet.ProductsOnPallet.Single().ProductId,
					Quantity: pallet.ProductsOnPallet.Single().Quantity,
					BestBefore: pallet.ProductsOnPallet.Single().BestBefore,
					DateAdded: now
				);
				palletsToUpdate.Add(palletTT);
			}
			var addingPallets = existingReceipt.StartUpdateReceipt(palletsToUpdate, request.DTO.PerformedBy);
			var listOfNewPalletNumbers = await _palletNumberAllocator.ReserveAsync(addingPallets.Count, ct);

			for (var i = 0; i < addingPallets.Count; i++)
			{
				var newPalletNumber = listOfNewPalletNumbers[i];
				var palletToAdd = addingPallets[i];
				var pallet = Pallet.Create(newPalletNumber, request.DTO.RampNumber, now);
				pallet.AddProduct(palletToAdd.ProductId,palletToAdd.Quantity, now, palletToAdd.BestBefore);
				_palletRepo.AddPallet(pallet);
				pallet.AssignToReceipt(existingReceipt.Id, snapShot, request.DTO.PerformedBy);
				existingReceipt.AttachPallet(pallet);
			}
			existingReceipt.UpdateReceipt(request.DTO.PerformedBy, request.DTO.ClientId, now);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Receipt {existingReceipt.ReceiptNumber} was updated.");
		}
	}
}
