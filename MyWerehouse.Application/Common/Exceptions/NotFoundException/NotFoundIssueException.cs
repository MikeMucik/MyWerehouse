using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions.BuisnessRuleException;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundIssueException : NotFoundException
	{
		public int IssueId { get; }

		public NotFoundIssueException(int issueId)
			: base($"Zamówienie o numerze {issueId} nie zostało znalezione.")
		{
			IssueId = issueId;
		}
		public NotFoundIssueException(string message) : base(message) { }

		//public IssueException(string message, Exception inner) : base(message, inner) { }

	}
}
