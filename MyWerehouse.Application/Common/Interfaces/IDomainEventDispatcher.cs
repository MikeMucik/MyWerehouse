using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Application.Common.Interfaces
{
	public interface IDomainEventDispatcher
	{
		Task DispatchAsync(
		IReadOnlyCollection<IDomainEvent> domainEvents,
		CancellationToken ct);
	}
}
