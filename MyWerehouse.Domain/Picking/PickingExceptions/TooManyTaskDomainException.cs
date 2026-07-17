using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class TooManyTaskDomainException : DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public TooManyTaskDomainException(Guid issueId, Guid productId)
			: base("Too many tasks, only one can exist.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}
