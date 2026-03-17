using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Events;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking
{
	public class CreateHistoryReversePickingHandler :INotificationHandler<CreateHistoryReversePickingNotification>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IHistoryReversePickingRepo _historyReversePickingRepo;
		public CreateHistoryReversePickingHandler(WerehouseDbContext werehouseDbContext,
			IHistoryReversePickingRepo historyReversePickingRepo)
		{
			_werehouseDbContext = werehouseDbContext;
			_historyReversePickingRepo = historyReversePickingRepo;
		}
		public async Task Handle(CreateHistoryReversePickingNotification request, CancellationToken ct)
		{
			var history = new HistoryReversePicking
			{
				ReversePickingId = request.ReversePickingId,
				PalletSourceId = request.PalletSourceId,
				PalletDestinationId = request.PalletDestinationId,
				IssueId = request.IssueId,
				IssueNumber = request.IssueNumber,
				ProductId = request.ProductId,
				DateTime = DateTime.UtcNow,
				PerformedBy = request.UserId,
				Quantity = request.Quantity,
				StatusBefore = request.StatusBefore,
				StatusAfter = request.StatusAfter,
			};
			await _historyReversePickingRepo.AddHistoryReversePickingAsync(history, ct);			
		}
	}
}
