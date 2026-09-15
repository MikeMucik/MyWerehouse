using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.ReversePickings.Events;

namespace MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking
{
	public class CreateHistoryReversePickingHandler
		: INotificationHandler<DomainEventNotification<CreateHistoryReversePickingNotification>>
	{
		private readonly IHistoryReversePickingRepo _historyReversePickingRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IDateTimeProvider _dateTimeProvider;
		public CreateHistoryReversePickingHandler(IHistoryReversePickingRepo historyReversePickingRepo, IPalletRepo pallet, IDateTimeProvider dateTimeProvider)
		{
			_historyReversePickingRepo = historyReversePickingRepo;
			_palletRepo = pallet;
			_dateTimeProvider = dateTimeProvider;
		}
		public async Task Handle(DomainEventNotification<CreateHistoryReversePickingNotification> notification, CancellationToken ct)
		{
			var domaintEvent = notification.DomainEvent;
			var sourceTask = domaintEvent.PalletSourceId != null
				? _palletRepo.GetPalletByIdAsync(domaintEvent.PalletSourceId.Value, ct)
				: Task.FromResult<Pallet?>(null);
			var destinationTask = domaintEvent.PalletDestinationId != null
				? _palletRepo.GetPalletByIdAsync(domaintEvent.PalletDestinationId.Value, ct)
				: Task.FromResult<Pallet?>(null);
			var pickingTask = _palletRepo.GetPalletByIdAsync(domaintEvent.PickingPalletId, ct);
			await Task.WhenAll(sourceTask, destinationTask, pickingTask);

			var sourcePallet = sourceTask.Result;
			var destinationPallet = destinationTask.Result;
			var pickingPallet = pickingTask.Result;
			ArgumentNullException.ThrowIfNull(pickingPallet);
			var history = new HistoryReversePicking
				{
					ReversePickingId = domaintEvent.ReversePickingId,
					PickingPalletId = domaintEvent.PickingPalletId,
					PickingPalletNumber = pickingPallet.PalletNumber,
					PalletSourceId = domaintEvent.PalletSourceId,
					PalletSourceNumber = sourcePallet?.PalletNumber,
					PalletDestinationId = domaintEvent.PalletDestinationId,
					PalletDestinationNumber = destinationPallet?.PalletNumber,
					IssueId = domaintEvent.IssueId,
					IssueNumber = domaintEvent.IssueNumber,
					ProductId = domaintEvent.ProductId,
					DateTime = _dateTimeProvider.UtcNow,
					PerformedBy = domaintEvent.UserId,
					Quantity = domaintEvent.Quantity,
					StatusBefore = domaintEvent.StatusBefore,
					StatusAfter = domaintEvent.StatusAfter,
				};
			_historyReversePickingRepo.AddHistoryReversePicking(history);
		}
	}
}
