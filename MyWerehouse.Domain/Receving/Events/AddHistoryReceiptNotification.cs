using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Receviving.Events
{
	public record AddHistoryReceiptNotification(
		Guid ReceiptId,
		int ReceiptNumber,
		int ClientId,
		ReceiptStatus ReceiptStatus,
		string UserId,
		IReadOnlyCollection<HistoryReceiptIssueDetailDto> DetailDtos) : IDomainEvent;

}
