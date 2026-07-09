using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Receiving.Models;

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
