using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking
{
	public class CreateHistoryPickingHandler(IHistoryPickingRepo historyPickingRepo,
		WerehouseDbContext werehouseDbContext) : INotificationHandler<CreateHistoryPickingNotification>
	{
		private readonly IHistoryPickingRepo _historyPickingRepo = historyPickingRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task Handle(CreateHistoryPickingNotification request, CancellationToken ct)
		{			
			var history = new HistoryPicking
			{
				AllocationId = request.DataPicking.AllocationId,
				PalletId = request.DataPicking.PalletId,
				IssueId = request.DataPicking.IssueId,
				ProductId = request.DataPicking.ProductId,
				QuantityAllocated = request.DataPicking.QuantityAllocated,
				QuantityPicked = request.DataPicking.QuantityPicked,
				StatusBefore = request.DataPicking.StatusBefore,
				StatusAfter = request.DataPicking.StatusAfter,
				PerformedBy = request.DataPicking.PerformedBy,
				DateTime = DateTime.UtcNow,
			};
			await _historyPickingRepo.AddHistoryPickingAsync(history, ct);
			await _werehouseDbContext.SaveChangesAsync(ct);
		}
	}
}
