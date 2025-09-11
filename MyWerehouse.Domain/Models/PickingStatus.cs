using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public enum PickingStatus
	{
		Available = 0,
		Allocated = 1,
		Picked = 2,
	}
}
//TODO
//HistoryAllocation

//Id

//VirtualPalletId

//AllocationStatus (Allocated, Picked, Archived, …)

//PerformedBy

//DateTime

//HistoryAllocationDetail

//Id

//HistoryAllocationId

//IssueId (albo IssueDetailId, jeśli masz takie rozbicie)

//ProductId

//QuantityAllocated

//QuantityPicked