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
	//public record HistoryDataPicking(
	//	int? PickingTaskId,
	//	string PalletId,
	//	Guid IssueId,
	//	int IssueNumber,
	//	int ProductId,
	//	int QuantityAllocated,
	//	int QuantityPicked,
	//	PickingStatus StatusBefore,
	//	PickingStatus StatusAfter,
	//	string PerformedBy,
	//	DateTime DateTime);

	//public record CreateHistoryPickingNotification(
	//	HistoryDataPicking DataPicking) : INotification;
	public record CreateHistoryPickingNotification(
		Guid PickingTaskId,
		//int PickingTaskNumber,
		string PalletId,
		Guid IssueId,
		int IssueNumber,
		int ProductId,
		int QuantityAllocated,
		int QuantityPicked,
		PickingStatus StatusBefore,
		PickingStatus StatusAfter,
		string PerformedBy,
		DateTime DateTime) : IDomainEvent;	
}
