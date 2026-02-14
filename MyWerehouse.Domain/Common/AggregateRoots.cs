using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Domain.Common
{
	public interface IDomainEvent : INotification { }
	public class AggregateRoots
	{
		private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
		public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
		protected void AddDomainEvent(IDomainEvent domainEvent)
		{
			_domainEvents.Add(domainEvent);
		}
		public void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}
	}
}
