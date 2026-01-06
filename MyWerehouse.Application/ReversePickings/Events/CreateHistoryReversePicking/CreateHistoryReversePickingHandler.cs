using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

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
			//Exceptions!!
			var history = new HistoryReversePicking
			{
				ReversePickingId = request.History.ReversePickingId,
				PalletSourceId = request.History.PalletSourceId,
				PalletDestinationId = request.History.PalletDestinationId,
				IssueId = request.History.IssueId,
				ProductId = request.History.ProductId,
				DateTime = DateTime.UtcNow,
				PerformedBy = request.UserId,
				Quantity = request.History.Quantity,
				StatusBefore = request.History.StatusBefore,
				StatusAfter = request.History.StatusAfter,
			};
			await _historyReversePickingRepo.AddHistoryReversePickingAsync(history, ct);
			await _werehouseDbContext.SaveChangesAsync(ct);
		}
	}
}
