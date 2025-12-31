using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;

namespace MyWerehouse.Application.Common.Commands
{
	public class CommandCollector : ICommandCollector
	{
		private readonly List<IRequest<Unit>> _request = new();
		public IReadOnlyCollection<IRequest<Unit>> Requests => _request.AsReadOnly();
		public void Add(IRequest<Unit> request)
		{
			_request.Add(request);

		}
		public void Clear()
		{
			_request.Clear();
		}
	}
}