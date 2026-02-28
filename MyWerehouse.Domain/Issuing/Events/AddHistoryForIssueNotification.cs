using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receviving.Events;

namespace MyWerehouse.Domain.Issuing.Events
{
	public record AddHistoryForIssueNotification(
		Guid IssueId,
		int IssueNumber, 
		int ClientId,
		IssueStatus IssueStatus,
		string UserId,
		IReadOnlyCollection<HistoryReceiptIssueDetailDto> DetailDtos,
		IReadOnlyCollection<AddListItemsOfIssueDetailsDto> Detailsitems) : IDomainEvent;//do poprawy jak w receipt
}
