using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.Events
{
	public record AddHistoryForIssueNotification(int IssueId, string PerformedBy) : IDomainEvent;
}
