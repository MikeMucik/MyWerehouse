using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class NotEndedLoadingException : DomainException
	{
		public Guid IssueId { get; }
		public NotEndedLoadingException(Guid issueId)
			: base($"Issue {issueId} has pallets not fully loaded.")
		{
			IssueId = issueId;

		}
	}
}
