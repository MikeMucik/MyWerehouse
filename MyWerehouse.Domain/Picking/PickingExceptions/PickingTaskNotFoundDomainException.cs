using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class PickingTaskNotFoundDomainException : DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public PickingTaskNotFoundDomainException(Guid issueId, Guid productId)
			: base($"Not found task.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}
