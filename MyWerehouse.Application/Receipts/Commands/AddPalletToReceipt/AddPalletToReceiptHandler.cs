using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Receviving.Events;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo,
		IPublisher mediator,
		IMapper mapper,
		IProductRepo productRepo,
		ILocationRepo locationRepo
			) : IRequestHandler<AddPalletToReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPublisher _mediator = mediator;
		private readonly IMapper _mapper = mapper;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<ReceiptResult> Handle(AddPalletToReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
			?? throw new NotFoundReceiptException(request.ReceiptId);
			var rampNumber = receipt.RampNumber;			
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct))
			{				
				try
				{
					receipt.StartReceiving(DateTime.UtcNow, request.DTO.UserId);
					var newId = await _palletRepo.GetNextPalletIdAsync();
					var location = await _locationRepo.GetLocationByIdAsync(rampNumber)
						?? throw new NotFoundLocationException(rampNumber);
					var pallet = new Pallet(newId, DateTime.UtcNow, rampNumber, location);					
					foreach (var dto in request.DTO.ProductsOnPallet)
					{
						if (!await _productRepo.IsExistProduct(dto.ProductId)) throw new NotFoundProductException(dto.ProductId);
						pallet.AddProduct(dto.ProductId, dto.Quantity, dto.BestBefore);
					}					
					pallet.AssignToReceipt(receipt.Id, request.DTO.UserId);
					_palletRepo.AddPallet(pallet);
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					return ReceiptResult.Ok($"Paleta {pallet.Id} została dodana do przyjęcia {request.ReceiptId}", pallet.Id);					
				}
				catch (NotFoundProductException epr)
				{
					await transaction.RollbackAsync(ct);
					return ReceiptResult.Fail(epr.Message);
				}
				catch (NotFoundReceiptException erp)
				{
					await transaction.RollbackAsync(ct);
					return ReceiptResult.Fail(erp.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync(ct);
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
	}
}
//if (receipt.ReceiptStatus == ReceiptStatus.Planned)
//{
//	receipt.ReceiptStatus = ReceiptStatus.InProgress;
//	receipt.ReceiptDateTime = DateTime.UtcNow;
//	startReceiving = true;
//}

//var listOfProduct = new List<ProductOnPallet>();
//pallet.ApplyProductChanges()

//foreach (var newProduct in request.DTO.ProductsOnPallet)
//{

//	var newProductToPallet = new ProductOnPallet
//	{
//		ProductId = newProduct.ProductId,
//		Quantity = newProduct.Quantity,
//		BestBefore = newProduct.BestBefore,
//		DateAdded = DateTime.UtcNow,
//	};
//	listOfProduct.Add(newProductToPallet);
//}

//var palletToReceipt = new Pallet(newId, DateTime.UtcNow, listOfProduct);

//palletToReceipt.LocationId = rampNumber;
//palletToReceipt.AssignToReceipt(receipt.Id, request.DTO.UserId);

//if (receipt.ReceiptStatus != ReceiptStatus.Planned && receipt.ReceiptStatus != ReceiptStatus.InProgress)
//{
//	return ReceiptResult.Fail("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
//}


//var pallet = _mapper.Map<Pallet>(request.DTO);

//pallet.ReceiptId = request.ReceiptId;
//pallet.Id = newId; // await _palletRepo.GetNextPalletIdAsync();//kolejny numer palety
//pallet.LocationId = rampNumber;//lokalizacja początkowa
//pallet.DateReceived = DateTime.UtcNow;
////pallet.Status = PalletStatus.Receiving;
//if (!await _productRepo.IsExistProduct(request.DTO.ProductsOnPallet.First().ProductId))
//	throw new NotFoundProductException(request.DTO.ProductsOnPallet.First().ProductId);




//_palletRepo.AddPallet(palletToReceipt);

//pallet.AssignToReceipt(receipt.Id, pallet.Location, request.DTO.UserId);



//if (startReceiving)
//{
//	await _mediator.Publish(new ChangeStatusReceiptNotification(receipt.Id, receipt.ReceiptStatus, request.DTO.UserId), ct);
//}

//var startReceiving = false;