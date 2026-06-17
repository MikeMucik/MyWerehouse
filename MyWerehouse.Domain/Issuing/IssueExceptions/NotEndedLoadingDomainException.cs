using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class NotEndedLoadingDomainException : DomainException
	{
		public Guid IssueId { get; }
		public int IssueNumber { get; }
		public NotEndedLoadingDomainException(Guid issueId, int issueNumber)
			: base($"Issue {issueNumber}({issueId}) has pallets not fully loaded.")
		{
			IssueId = issueId;
			IssueNumber = issueNumber;
		}
	}
}
