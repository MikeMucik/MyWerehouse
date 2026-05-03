using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		ILocationRepo locationRepo
			) : IRequestHandler<AddPalletToReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<AppResult<Unit>> Handle(AddPalletToReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);
			var rampNumber = receipt.RampNumber;
			receipt.StartReceiving(DateTime.UtcNow, request.DTO.UserId);
			var newId = await _palletRepo.GetNextPalletIdAsync();

			var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
			if (location == null) return AppResult<Unit>.Fail($"Lokalizacja o numerze {rampNumber} nie została znaleziona", ErrorType.NotFound);

			var pallet = Pallet.Create(newId, rampNumber);
			//Jest dla wielu choć początkowe założenia mówiły o jednym produkcie na palecie - szerszse podejście
			foreach (var dto in request.DTO.ProductsOnPallet)
			{
				if (!await _productRepo.IsExistProduct(dto.ProductId))
					return AppResult<Unit>.Fail($"Produkt o numerze {dto.ProductId} nie istnieje.", ErrorType.NotFound);
				pallet.AddProduct(dto.ProductId, dto.Quantity, dto.BestBefore);
			}
			var snapShot = location.ToSnapshot();
			pallet.AssignToReceipt(receipt.Id, snapShot, request.DTO.UserId);
			_palletRepo.AddPallet(pallet);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Paleta {pallet.Id} została dodana do przyjęcia {request.ReceiptId}");
		}
	}
}