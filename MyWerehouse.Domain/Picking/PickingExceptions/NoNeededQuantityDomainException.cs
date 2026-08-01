using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class NoNeededQuantityDomainException :DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public NoNeededQuantityDomainException(Guid issueId, Guid productId)
			:base($"The selected issue {issueId} has no remaining demand for this product{productId}.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}
