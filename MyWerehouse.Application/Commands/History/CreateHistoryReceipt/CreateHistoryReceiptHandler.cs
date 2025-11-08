using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Commands.History.CreateHistoryReceipt
{
	public class CreateHistoryReceiptHandler :INotificationHandler<CreateHistoryReceiptCommand>
	{
		private IReceiptRepo _receiptRepo;
		private IHistoryReceiptRepo _historyReceiptRepo;
		public CreateHistoryReceiptHandler(IReceiptRepo receiptRepo,
			IHistoryReceiptRepo historyReceiptRepo)
		{
			_receiptRepo = receiptRepo;
			_historyReceiptRepo = historyReceiptRepo;
		}
		public async Task Handle(CreateHistoryReceiptCommand request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
				?? throw new ReceiptNotFoundException(request.ReceiptId);
			var details = (receipt.Pallets != null && receipt.Pallets.Count != 0)
				? receipt.Pallets.Select(p => new HistoryReceiptDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
					LocationSnapShot = $"{p.Location.Bay}-{p.Location.Aisle}-{p.Location.Position}-{p.Location.Height}"
				}).ToList() : new List<HistoryReceiptDetail>();
			var history = new HistoryReceipt
			{
				Receipt = receipt,
				ClientId = receipt.ClientId,
				StatusAfter =request.ReceiptStatus,
				PerformedBy = request.UserId,
				DateTime = DateTime.UtcNow,
				Details = details
			};
			await _historyReceiptRepo.AddHistoryReceiptAsync(history, cancellationToken);
			await _historyReceiptRepo.SaveChanges();
		}
	}
}
