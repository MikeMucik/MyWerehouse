using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Exceptions
{
	public class OrderNotFoundException : Exception
	{
		public int IssueId { get; }

		public OrderNotFoundException(int issueId)
			: base($"Zamówienie o numerze {issueId} nie zostało znalezione.")
		{
			IssueId = issueId;
		}
		public OrderNotFoundException(string message) : base(message) { }

		public OrderNotFoundException(string message, Exception inner) : base(message, inner) { }

	}
}
