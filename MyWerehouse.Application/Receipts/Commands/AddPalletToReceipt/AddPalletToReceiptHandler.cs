using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		ILocationRepo locationRepo,
		IDateTimeProvider dateTimeProvider
			) : IRequestHandler<AddPalletToReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<Unit>> Handle(AddPalletToReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");
			var rampNumber = receipt.RampNumber;
			var now = _dateTimeProvider.UtcNow;
			receipt.StartReceiving(now, request.DTO.UserId);
			var newId = await _palletRepo.GetNextPalletIdAsync();

			var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
			if (location == null) return AppResult<Unit>.Fail($"Location {rampNumber} was not found.");
			
			var pallet = Pallet.Create(newId, rampNumber, now);
			if (request.DTO.ProductsOnPallet.Count != 1)
			{
				return AppResult<Unit>.Fail($"A receiving pallet can contain only one product.", ErrorType.Conflict);
			}
			var product = request.DTO.ProductsOnPallet.Single();

			if (!await _productRepo.IsExistProduct(product.ProductId))
				return AppResult<Unit>.Fail($"Product {product.ProductId} does not exist.");

			pallet.AddProduct(product.ProductId, product.Quantity, now, product.BestBefore);
			
			var snapShot = location.ToSnapshot();
			pallet.AssignToReceipt(receipt.Id, snapShot, request.DTO.UserId);
			_palletRepo.AddPallet(pallet);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Pallet {pallet.Id} was added to receipt {request.ReceiptId}.");
		}
	}
}
