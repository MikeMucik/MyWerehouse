using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class EmptyListPalletsDoamainException :DomainException
	{
		public Guid IssueId { get; }
		public int IssueNumber { get; }

		public EmptyListPalletsDoamainException(Guid issueId, int issueNumber) :
			base($"List pallets is empty for issue {issueId}, {issueNumber}")
		{
			IssueId = issueId;
			IssueNumber = issueNumber;
		}
	}
}
