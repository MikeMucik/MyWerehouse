using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	internal class InvalidPickingStrategyDomainException : DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public InvalidPickingStrategyDomainException(Guid issueId, Guid productId)
			: base($"Not proper picking strategy.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}