using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.Common.Events
{
	public interface IEventCollector
	{
		void Add(INotification @event);
		//void AddDeferred(Func<Task<INotification>> eventFactory);
		void AddDeferred(Func<INotification> eventFactory);
		IReadOnlyCollection<INotification> Events { get; }
		//IReadOnlyCollection<Func<Task<INotification>>> DeferredEvents { get; }
		IReadOnlyCollection<Func<INotification>> DeferredEvents { get; }
		void Clear();
	}
}
