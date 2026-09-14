using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Picking.Events;

namespace MyWerehouse.Application.Picking.Events.CreateHistoryPicking
{
	public class CreateHistoryPickingHandler(IHistoryPickingRepo historyPickingRepo, IDateTimeProvider dateTimeProvider)
		: INotificationHandler<DomainEventNotification<CreateHistoryPickingNotification>>
	{
		private readonly IHistoryPickingRepo _historyPickingRepo = historyPickingRepo;		
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public Task Handle(DomainEventNotification<CreateHistoryPickingNotification> request, CancellationToken ct)
		{
			var domaintEvent = request.DomainEvent;
			var history = new HistoryPicking
			{			
				PickingTaskId = domaintEvent.PickingTaskId,
				IssueNumber = domaintEvent.IssueNumber,
				PalletId = domaintEvent.PalletId,
				PalletNumber = domaintEvent.PalletNumber,
				PickingPalletId = domaintEvent.PickingPalleId,
				PickingPalletNumber = domaintEvent.PickingPalletNumber,
				IssueId = domaintEvent.IssueId,
				ProductId = domaintEvent.ProductId,
				QuantityAllocated = domaintEvent.QuantityAllocated,
				QuantityPicked = domaintEvent.QuantityPicked,
				StatusBefore = domaintEvent.StatusBefore,
				StatusAfter = domaintEvent.StatusAfter,
				PerformedBy = domaintEvent.PerformedBy,
				DateTime = _dateTimeProvider.UtcNow,
			};
			_historyPickingRepo.AddHistoryPicking(history);		
			return Task.CompletedTask;
		}
	}
}
