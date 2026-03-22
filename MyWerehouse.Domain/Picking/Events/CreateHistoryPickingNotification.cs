using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Picking.Events
{	
	public record CreateHistoryPickingNotification(
		Guid PickingTaskId,
		//int PickingTaskNumber,
		Guid PalletId,
		string PalletNumber,
		Guid IssueId,
		int IssueNumber,
		Guid ProductId,
		int QuantityAllocated,
		int QuantityPicked,
		PickingStatus StatusBefore,
		PickingStatus StatusAfter,
		string PerformedBy,
		DateTime DateTime) : IDomainEvent;	
}
