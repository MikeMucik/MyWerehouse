using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class NotAllowedOperationException : DomainException
	{
		public Guid IssueId { get; }
		public NotAllowedOperationException(Guid issueId)
			: base($"Operation forbidden for {issueId}, wrong status.")
		{ 
			IssueId = issueId;
		}
	}
}
