using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class CannotCancelPickingTaskInCurrentStatusException :DomainException
	{
		public Guid PickingTaskId { get; }
		public Guid IssueId { get; }
		public PickingStatus PickingStatus { get; }
		public CannotCancelPickingTaskInCurrentStatusException(Guid pickingTaskId, Guid issueId, PickingStatus pickingStatus) :
			base($"Cannot cancel picking task { pickingTaskId} in status '{pickingStatus}' number issue {issueId}.")
		{
			PickingTaskId = pickingTaskId;
			IssueId = issueId;
			PickingStatus = pickingStatus;
		}
	}
}
