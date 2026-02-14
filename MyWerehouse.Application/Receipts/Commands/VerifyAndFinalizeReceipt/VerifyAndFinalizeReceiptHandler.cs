using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Receviving.Events;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public class VerifyAndFinalizeReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IMediator mediator) : IRequestHandler<VerifyAndFinalizeReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPublisher _mediator = mediator;

		public async Task<ReceiptResult> Handle(VerifyAndFinalizeReceiptCommand request, CancellationToken cancellationToken)
		{
			//TODO :MOże dodać porównanie papierów z tym co rzeczywiście przyjęte, compare amount assignment to real receipt
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(cancellationToken);
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				return ReceiptResult.Fail("Nie można zweryfikować przyjęcia");
			}
			var palletsVerified = receipt.VerifiedReceipt();
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			var stockChanges = new List<StockItemChange>();
					
			foreach (var pallet in receipt.Pallets)
			{
				 stockChanges = [.. pallet.ProductsOnPallet
					.GroupBy(p => p.ProductId)
					.Select(g => new StockItemChange(g.Key, g.Sum(x => x.Quantity)))];
			}
			foreach (var pallet in receipt.Pallets)
			{
				await _mediator.Publish(new ChangeStatusOfPalletNotification(
						pallet.Id,
						pallet.LocationId,
						pallet.Location.ToSnopShot(),
						pallet.LocationId,
						pallet.Location.ToSnopShot(),
						ReasonMovement.Received,
						receipt.PerformedBy,
						PalletStatus.InStock,
						null
					), cancellationToken);				
			}			
			await _mediator.Publish(new ChangeStockNotification(StockChangeType.Increase, stockChanges), cancellationToken);
			await _mediator.Publish(new ChangeStatusReceiptNotification(request.ReceiptId, receipt.ReceiptStatus, request.UserId), cancellationToken);
			return ReceiptResult.Ok("Palety z przyjęcia zweryfikowano, gotowe do działania", request.ReceiptId);			
		}
	}
}
//var productQuantityChanges = new Dictionary<int, int>();	
//foreach (var pallet in receipt.Pallets)
//{
//foreach (var product in pallet.ProductsOnPallet)
//{
//	productQuantityChanges[product.ProductId] =
//		productQuantityChanges.GetValueOrDefault(product.ProductId) + product.Quantity;
//}
//	}
//await _mediator.Publish(new ChangeStockNotification(StockChangeType.Increase, productQuantityChanges.Select(k=>new StockItemChange( k.Key, k.Value))), cancellationToken);
