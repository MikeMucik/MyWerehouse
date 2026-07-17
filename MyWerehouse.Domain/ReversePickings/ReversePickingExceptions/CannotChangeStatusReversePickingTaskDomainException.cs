using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Domain.ReversePickings.ReversePickingExceptions
{
	public class CannotChangeStatusReversePickingTaskDomainException : DomainException
	{
		public Guid ReversePickingTaskId { get; }
		public ReversePickingStatus Status { get; }
		public CannotChangeStatusReversePickingTaskDomainException(Guid reversePickingTaskId, ReversePickingStatus status)
			: base($"Operation prohibited, wrong status: {status}")
		{
			ReversePickingTaskId = reversePickingTaskId;
			Status = status;
		}
	}
}
