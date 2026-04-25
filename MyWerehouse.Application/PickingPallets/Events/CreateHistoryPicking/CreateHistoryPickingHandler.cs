using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Events;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking
{
	public class CreateHistoryPickingHandler(IHistoryPickingRepo historyPickingRepo)
		: INotificationHandler<CreateHistoryPickingNotification>
	{
		private readonly IHistoryPickingRepo _historyPickingRepo = historyPickingRepo;		

		public async Task Handle(CreateHistoryPickingNotification request, CancellationToken ct)
		{			
			var history = new HistoryPicking
			{			
				PickingTaskId = request.PickingTaskId,
				IssueNumber = request.IssueNumber,
				PalletId = request.PalletId,
				PalletNumber = request.PalletNumber,
				PickingPalletId = request.PickingPalleId,
				PickingPalletNumber = request.PickingPalletNumber,
				IssueId = request.IssueId,
				ProductId = request.ProductId,
				QuantityAllocated = request.QuantityAllocated,
				QuantityPicked = request.QuantityPicked,
				StatusBefore = request.StatusBefore,
				StatusAfter = request.StatusAfter,
				PerformedBy = request.PerformedBy,
				DateTime = DateTime.UtcNow,
			};
			await _historyPickingRepo.AddHistoryPickingAsync(history, ct);			
		}
	}
}
