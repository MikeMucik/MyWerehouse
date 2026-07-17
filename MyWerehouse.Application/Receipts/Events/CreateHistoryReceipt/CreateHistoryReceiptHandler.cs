using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receiving.Events;

namespace MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt
{
	public class CreateHistoryReceiptHandler(IHistoryReceiptRepo historyReceiptRepo, IDateTimeProvider dateTimeProvider) : INotificationHandler<AddHistoryReceiptNotification>
	{		
		private readonly IHistoryReceiptRepo _historyReceiptRepo = historyReceiptRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public Task Handle(AddHistoryReceiptNotification request, CancellationToken ct)
		{			
			var details = request.DetailDtos ?? Enumerable.Empty<HistoryReceiptIssueDetailDto>();
			var history = new HistoryReceipt
			{
				ReceiptId = request.ReceiptId,
				ReceiptNumber= request.ReceiptNumber,
				ClientId = request.ClientId,
				StatusAfter = request.ReceiptStatus,
				PerformedBy = request.UserId,
				DateTime = _dateTimeProvider.UtcNow,
				Details = details
				.Select(d => new HistoryReceiptDetail
				{
					PalletId = d.PalletId,
					PalletNumber = d.PalletNumber,
					LocationId = d.LocationId,
					LocationSnapShot = d.LocationSnapShot,
				})
				.ToList() ?? new List<HistoryReceiptDetail>()
			};
			_historyReceiptRepo.AddHistoryReceipt(history);
			return Task.CompletedTask;
		}
	}
}
