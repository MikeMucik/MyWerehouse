using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class NotAllowedOperationDomainException : DomainException
	{
		public Guid IssueId { get; }
		public int IssueNumber { get; }
		public NotAllowedOperationDomainException(Guid issueId, int issueNumber)
			: base($"Operation forbidden for {issueNumber}({issueId}), wrong status.")
		{ 
			IssueId = issueId;
			IssueNumber = issueNumber;
		}
	}
}
