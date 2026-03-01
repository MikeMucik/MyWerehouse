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
	public record CreateHistoryReversePickingNotification(Guid ReversePickingId,
		string? PalletSourceId,
		string? PalletDestinationId,
		Guid IssueId,
		int IssueNumber,
		int ProductId,
		int Quantity,
		ReversePickingStatus? StatusBefore,
		ReversePickingStatus StatusAfter,
		string UserId) :IDomainEvent; 
}