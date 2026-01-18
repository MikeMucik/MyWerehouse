using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt
{
	public class CreateHistoryReceiptHandler :INotificationHandler<CreateHistoryReceiptNotification>
	{
		private readonly IReceiptRepo _receiptRepo;
		private readonly IHistoryReceiptRepo _historyReceiptRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public CreateHistoryReceiptHandler(IReceiptRepo receiptRepo,
			IHistoryReceiptRepo historyReceiptRepo,
			WerehouseDbContext werehouseDbContext)
		{
			_receiptRepo = receiptRepo;
			_historyReceiptRepo = historyReceiptRepo;
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task Handle(CreateHistoryReceiptNotification request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
				?? throw new NotFoundReceiptException(request.ReceiptId);
			var details = receipt.Pallets != null && receipt.Pallets.Count != 0
				? receipt.Pallets.Select(p => new HistoryReceiptDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
					LocationSnapShot = $"{p.Location.Bay}-{p.Location.Aisle}-{p.Location.Position}-{p.Location.Height}"
				}).ToList() : new List<HistoryReceiptDetail>();
			var history = new HistoryReceipt
			{
				ReceiptId = receipt.Id,
				ClientId = receipt.ClientId,
				StatusAfter =request.ReceiptStatus,
				PerformedBy = request.UserId,
				DateTime = DateTime.UtcNow,
				Details = details
			};
			await _historyReceiptRepo.AddHistoryReceiptAsync(history, cancellationToken);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
