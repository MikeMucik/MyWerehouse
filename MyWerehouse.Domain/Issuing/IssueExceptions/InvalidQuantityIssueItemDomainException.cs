using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class InvalidQuantityIssueItemDomainException : DomainException
	{
		public int Quantity { get; }
		public Guid IssueId { get; }
		public int IssueNumber { get; }
		public InvalidQuantityIssueItemDomainException(int quantity, Guid issueId, int issueNumber) :
			base($"Not allowed qunatity.")
		{
			Quantity = quantity;
			IssueId = issueId;
			IssueNumber = issueNumber;
		}
	}
}
