using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.Common.Events
{
	public class EventCollector : IEventCollector
	{
		private readonly List<INotification> _events = new();
		private readonly List<Func<Task<INotification>>> _deferred = new();
		public IReadOnlyCollection<INotification> Events => _events.AsReadOnly();

		public IReadOnlyCollection<Func<Task<INotification>>> DeferredEvents => _deferred.AsReadOnly();
		public void Add(INotification @event)=> _events.Add(@event);
		public void AddDeferred(Func<Task<INotification>> eventFactory)=> _deferred.Add(eventFactory);
		public void Clear()
		{
			_events.Clear();
			_deferred.Clear();
		} 		
	}
}
