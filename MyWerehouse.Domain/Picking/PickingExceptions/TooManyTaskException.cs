using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class TooManyTaskException :DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public TooManyTaskException(Guid issueId, Guid productId)
			: base($"Only one task has to be.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}
