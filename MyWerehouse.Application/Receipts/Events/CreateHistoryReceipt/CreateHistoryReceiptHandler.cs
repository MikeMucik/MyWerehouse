using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Receiving.Events;

namespace MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt
{
	public class CreateHistoryReceiptHandler(IHistoryReceiptRepo historyReceiptRepo, IDateTimeProvider dateTimeProvider) 
		: INotificationHandler<DomainEventNotification<AddHistoryReceiptNotification>>
	{		
		private readonly IHistoryReceiptRepo _historyReceiptRepo = historyReceiptRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public Task Handle( DomainEventNotification<AddHistoryReceiptNotification> request, CancellationToken ct)
		{
			var domaintEvent = request.DomainEvent;
			var details = domaintEvent.DetailDtos ?? Enumerable.Empty<HistoryReceiptIssueDetailDto>();
			var history = new HistoryReceipt
			{
				ReceiptId = domaintEvent.ReceiptId,
				ReceiptNumber= domaintEvent.ReceiptNumber,
				ClientId = domaintEvent.ClientId,
				StatusAfter = domaintEvent.ReceiptStatus,
				PerformedBy = domaintEvent.UserId,
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
