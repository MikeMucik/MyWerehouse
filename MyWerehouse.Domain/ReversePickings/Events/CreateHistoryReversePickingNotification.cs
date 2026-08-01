using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Domain.ReversePickings.Events
{
	public record CreateHistoryReversePickingNotification(
		Guid ReversePickingId,
		Guid PickingPalletId,
		Guid? PalletSourceId,		
		Guid? PalletDestinationId,
		Guid IssueId,
		int IssueNumber,
		Guid ProductId,
		int Quantity,
		ReversePickingStatus? StatusBefore,
		ReversePickingStatus StatusAfter,
		string UserId) :IDomainEvent; 
}
