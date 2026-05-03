using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	internal class InvalidPickingStrategyException : DomainException
	{
		public Guid IssueId { get; }
		public Guid ProductId { get; }
		public InvalidPickingStrategyException(Guid issueId, Guid productId)
			: base($"Not proper picking strategy.")
		{
			IssueId = issueId;
			ProductId = productId;
		}
	}
}
