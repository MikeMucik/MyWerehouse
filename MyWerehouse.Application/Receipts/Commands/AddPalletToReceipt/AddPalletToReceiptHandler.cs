using MediatR;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptHandler(IUnitOfWork unitOfWork,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		ILocationRepo locationRepo,
		IDateTimeProvider dateTimeProvider,
		IPalletNumberAllocator palletNumberAllocator
			) : IRequestHandler<AddPalletToReceiptCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		private readonly IPalletNumberAllocator _palletNumberAllocator = palletNumberAllocator;

		public async Task<AppResult<Unit>> Handle(AddPalletToReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId, ct);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");
			var rampNumber = receipt.RampNumber;
			var now = _dateTimeProvider.UtcNow;
			receipt.StartReceiving(now, request.DTO.UserId);
			var newId = (await _palletNumberAllocator.ReserveAsync(1, ct)).Single();

			var location = await _locationRepo.GetLocationByIdAsync(rampNumber, ct);
			if (location == null) return AppResult<Unit>.Fail($"Location {rampNumber} was not found.");

			var pallet = Pallet.Create(newId, rampNumber, now);
			if (request.DTO.ProductsOnPallet.Count != 1)
			{
				return AppResult<Unit>.Fail($"A receiving pallet can contain only one product.", ErrorType.Conflict);
			}
			var product = request.DTO.ProductsOnPallet.Single();

			if (!await _productRepo.IsExistProduct(product.ProductId, ct))
				return AppResult<Unit>.Fail($"Product {product.ProductId} does not exist.");

			pallet.AddProduct(product.ProductId, product.Quantity, now, product.BestBefore);

			var snapShot = location.ToSnapshot();
			pallet.AssignToReceipt(receipt.Id, snapShot, request.DTO.UserId);
			_palletRepo.AddPallet(pallet);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Pallet {pallet.Id} was added to receipt {request.ReceiptId}.");
		}
	}
}
