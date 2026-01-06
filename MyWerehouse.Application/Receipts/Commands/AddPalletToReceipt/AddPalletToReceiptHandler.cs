using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo,
		IPublisher mediator,
		IMapper mapper,
		IProductRepo productRepo
			) : IRequestHandler<AddPalletToReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPublisher _mediator = mediator;
		private readonly IMapper _mapper = mapper;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<ReceiptResult> Handle(AddPalletToReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
			??	throw new ReceiptException(request.ReceiptId);
			var rampNumber = receipt.RampNumber;
			if (receipt == null || (receipt.ReceiptStatus != ReceiptStatus.Planned && receipt.ReceiptStatus != ReceiptStatus.InProgress))
			{
				return ReceiptResult.Fail("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
			}
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct))
			{
				var startReceiving = false;
				try
				{
					if (receipt.ReceiptStatus == ReceiptStatus.Planned)
					{
						receipt.ReceiptStatus = ReceiptStatus.InProgress;
						receipt.ReceiptDateTime = DateTime.UtcNow;
						startReceiving = true;
					}
					var pallet = _mapper.Map<Pallet>(request.DTO);
					pallet.ReceiptId = request.ReceiptId;
					pallet.Id = await _palletRepo.GetNextPalletIdAsync();//kolejny numer palety
					pallet.LocationId = rampNumber;//lokalizacja początkowa
					pallet.DateReceived = DateTime.UtcNow;
					pallet.Status = PalletStatus.Receiving;
					if (!await _productRepo.IsExistProduct(request.DTO.ProductsOnPallet.First().ProductId))
						throw new NotFoundProductException(request.DTO.ProductsOnPallet.First().ProductId);
					_palletRepo.AddPallet(pallet);
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);

					await _mediator.Publish(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
							ReasonMovement.Received, request.DTO.UserId, PalletStatus.Receiving, null), ct);
					if (startReceiving)
					{
						await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, request.DTO.UserId), ct);
					}					
					return ReceiptResult.Ok($"Paleta {pallet.Id} została dodana do przyjęcia {request.ReceiptId}", pallet.Id);
				}
				catch(NotFoundProductException epr)
				{
					await transaction.RollbackAsync(ct);
					return ReceiptResult.Fail(epr.Message);
				}
				catch (ReceiptException erp)
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
