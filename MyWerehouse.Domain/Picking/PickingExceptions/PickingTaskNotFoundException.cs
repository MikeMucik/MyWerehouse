using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class PickingTaskNotFoundException : DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public PickingTaskNotFoundException(Guid issueId, Guid productId)
			: base($"Not found task.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}
