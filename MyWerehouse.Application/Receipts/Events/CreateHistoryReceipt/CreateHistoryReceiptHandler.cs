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
using MyWerehouse.Domain.Receviving.Events;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt
{
	public class CreateHistoryReceiptHandler(IHistoryReceiptRepo historyReceiptRepo) : INotificationHandler<AddHistoryReceiptNotification>
	{		
		private readonly IHistoryReceiptRepo _historyReceiptRepo = historyReceiptRepo;

		public async Task Handle(AddHistoryReceiptNotification request, CancellationToken ct)
		{			
			var details = request.DetailDtos ?? Enumerable.Empty<HistoryReceiptIssueDetailDto>();
			var history = new HistoryReceipt
			{
				ReceiptId = request.ReceiptId,
				ReceiptNumber= request.ReceiptNumber,
				ClientId = request.ClientId,
				StatusAfter = request.ReceiptStatus,
				PerformedBy = request.UserId,
				DateTime = DateTime.UtcNow,
				Details = details
				.Select(d => new HistoryReceiptDetail
				{
					PalletId = d.PalletId,
					LocationId = d.LocationId,
					LocationSnapShot = d.LocationSnapShot,
				})
				.ToList() ?? new List<HistoryReceiptDetail>()
			};
			await _historyReceiptRepo.AddHistoryReceiptAsync(history, ct);
		}
	}
}
