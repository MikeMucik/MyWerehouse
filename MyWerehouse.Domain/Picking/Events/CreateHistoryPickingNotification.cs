using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Picking.Events
{	
	public record CreateHistoryPickingNotification(
		Guid PickingTaskId,
		Guid? PalletId,//source
		string? PalletNumber,//source
		Guid? PickingPalleId,
		string? PickingPalletNumber,
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
