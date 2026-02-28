using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking
{
	public record HistoryReversePickingItem(
		int ReversePickingId,
		string? PalletSourceId,
		string? PalletDestinationId,
		Guid IssueId,
		int IssueNumber,
		int ProductId,
		int Quantity,
		ReversePickingStatus? StatusBefore,
		ReversePickingStatus StatusAfter);	
	public record CreateHistoryReversePickingNotification(HistoryReversePickingItem History,string UserId) : INotification;
}