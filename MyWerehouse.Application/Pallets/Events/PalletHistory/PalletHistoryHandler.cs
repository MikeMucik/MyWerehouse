using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;

namespace MyWerehouse.Application.Pallets.Events.PalletHistory
{
	public class PalletHistoryHandler(IHistoryPalletRepo palletMovementRepo, IDateTimeProvider dateTimeProvider)
		: INotificationHandler<DomainEventNotification<PalletHistoryNotification>>
	{
		private readonly IHistoryPalletRepo _palletMovementRepo = palletMovementRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public Task Handle(DomainEventNotification<PalletHistoryNotification> notification, CancellationToken cancellationToken)
		{
			var domaintEvent = notification.DomainEvent;
			var movement = new HistoryPallet
			{
				PalletId = domaintEvent.PalletId,
				PalletNumber = domaintEvent.PalletNumber,
				SourceLocationId = domaintEvent.SourceLocationId,
				SourceLocationSnapShot = domaintEvent.SourceSnapshot,
				DestinationLocationId = domaintEvent.DestinationLocationId,
				DestinationLocationSnapShot = domaintEvent.DestinationSnapshot,
				Reason = domaintEvent.ReasonMovement,
				PerformedBy = domaintEvent.UserId,
				MovementDate = _dateTimeProvider.UtcNow,
				PalletStatus = domaintEvent.PalletStatus,

				HistoryPalletDetails = domaintEvent.Details
				.Select(d => new HistoryPalletDetail
				{
					ProductId = d.ProductId,
					QuantityChange = d.QuantityChange,
				})
				.ToList(),
			};
			_palletMovementRepo.AddHistoryPallet(movement);
			return Task.CompletedTask;
		}
	}
}
