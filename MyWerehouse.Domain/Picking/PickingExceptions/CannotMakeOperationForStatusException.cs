using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class CannotMakeOperationForStatusException : DomainException
	{
		public Guid PickingTaskId { get; }
		public PickingStatus PickingStatus { get; }
		public CannotMakeOperationForStatusException(Guid pickingTaskId, PickingStatus pickingStatus)
			:base($"Operation for task {pickingTaskId} is prohibited, status {pickingStatus}.")
		{
			PickingTaskId = pickingTaskId;
			PickingStatus = pickingStatus;
		}
	}
}
