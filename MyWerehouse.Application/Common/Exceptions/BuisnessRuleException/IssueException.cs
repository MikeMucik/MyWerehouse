using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.BuisnessRuleException
{
	public class IssueException : BusinessRuleException
	{
		public int IssueId { get; }

		
		public IssueException(int issueId)
			: base($"Zamówienie o numerze {issueId} nie zostało znalezione.")
		{
			IssueId = issueId;
		}
		public 
			IssueException(string message) : base(message) { }

		//public IssueException(string message, Exception inner) : base(message, inner) { }
	}
}
