using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Application.Common.Events
{
	public sealed record DomainEventNotification<TDomainEvent>(
		TDomainEvent DomainEvent) : INotification
		where TDomainEvent : IDomainEvent;	
}
