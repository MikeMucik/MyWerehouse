using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Infrastructure.Common.Events
{
	public class MediatRDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
	{
		public async Task DispatchAsync(
		IReadOnlyCollection<IDomainEvent> domainEvents,
		CancellationToken ct)
		{
			foreach (var domainEvent in domainEvents)
			{
				var wrapperType = typeof(DomainEventNotification<>)
					.MakeGenericType(domainEvent.GetType());

				var notification = (INotification)Activator.CreateInstance(
					wrapperType,
					domainEvent)!;

				await publisher.Publish(notification, ct);
			}
		}
	}
}
