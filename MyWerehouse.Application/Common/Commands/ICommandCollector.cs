using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.Common.Commands
{
	public interface ICommandCollector
	{
		void Add(IRequest<Unit> request);
		//void AddDeferred(Func<Task<IRequest>> requestFactory);
		IReadOnlyCollection<IRequest<Unit>> Requests { get; }
		//IReadOnlyCollection<Func<Task<IRequest<Unit>>>> DeferredRequest { get; }
		void Clear();
	}
}
