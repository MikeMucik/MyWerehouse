using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receving.Events;

namespace MyWerehouse.Domain.Receiving.Events
{
	public record AddHistoryReceiptNotification(
		Guid ReceiptId,
		int ReceiptNumber,
		int ClientId,
		ReceiptStatus ReceiptStatus,
		string UserId,
		IReadOnlyCollection<HistoryReceiptIssueDetailDto> DetailDtos) : IDomainEvent;

}
