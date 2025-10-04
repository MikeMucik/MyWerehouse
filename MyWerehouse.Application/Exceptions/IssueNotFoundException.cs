using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Exceptions
{
	public class IssueNotFoundException : Exception
	{
		public int IssueId { get; }

		public IssueNotFoundException(int issueId)
			: base($"Zamówienie o numerze {issueId} nie zostało znalezione.")
		{
			IssueId = issueId;
		}
		public IssueNotFoundException(string message) : base(message) { }

		public IssueNotFoundException(string message, Exception inner) : base(message, inner) { }

	}
}
