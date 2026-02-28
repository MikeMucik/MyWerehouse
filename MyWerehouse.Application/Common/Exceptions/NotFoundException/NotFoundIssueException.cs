using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundIssueException : NotFoundException
	{
		public Guid IssueId { get; }
		public int IssueNumber { get; }
		public NotFoundIssueException(Guid issueId)
			: base($"Zamówienie o numerze {issueId} nie zostało znalezione.")
		{
			IssueId = issueId;
		}
		public NotFoundIssueException(int issueNumber)
			: base($"Zamówienie o numerze {issueNumber} nie zostało znalezione.")
		{
			IssueNumber = issueNumber;
		}
		public NotFoundIssueException(string message) : base(message) { }

		//public IssueException(string message, Exception inner) : base(message, inner) { }

	}
}
