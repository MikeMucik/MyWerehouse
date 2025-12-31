using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Inventories.Commands.ChangeQuantity;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public class VerifyAndFinalizeReceiptHandler : IRequestHandler<VerifyAndFinalizeReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IPublisher _mediator;
		public VerifyAndFinalizeReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_mediator = mediator;
		}
		public async Task<ReceiptResult> Handle(VerifyAndFinalizeReceiptCommand request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				return ReceiptResult.Fail("Nie można zweryfikować przyjęcia");
			}
			receipt.ReceiptStatus = ReceiptStatus.Verified;
			receipt.ReceiptDateTime = DateTime.UtcNow;
			foreach (var pallet in receipt.Pallets)
			{
				pallet.Status = PalletStatus.InStock;
			}
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);

			var productQuantityChanges = new Dictionary<int, int>();
			foreach (var pallet in receipt.Pallets)
			{
				foreach (var product in pallet.ProductsOnPallet)
				{
					productQuantityChanges[product.ProductId] =
						productQuantityChanges.GetValueOrDefault(product.ProductId) + product.Quantity;
				}
			}
			foreach (var pallet in receipt.Pallets)
			{
				await _mediator.Publish(new CreatePalletOperationNotification(
						pallet.Id,
						pallet.LocationId,
						ReasonMovement.Received,
						receipt.PerformedBy,
						PalletStatus.InStock,
						null
					), cancellationToken);				
			}			
			await _mediator.Publish(new ChangeStockNotification(StockChangeType.Increase, productQuantityChanges.Select(k=>new StockItemChange( k.Key, k.Value))), cancellationToken);
			await _mediator.Publish(new CreateHistoryReceiptNotification(request.ReceiptId, receipt.ReceiptStatus, request.UserId), cancellationToken);
			return ReceiptResult.Ok("Palety z przyjęcia zweryfikowano, gotowe do działania", request.ReceiptId);			
		}
	}
}
