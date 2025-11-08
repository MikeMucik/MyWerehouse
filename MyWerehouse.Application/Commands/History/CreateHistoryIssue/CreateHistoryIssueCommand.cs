using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Commands.History.CreateHistoryIssue
{
	public record CreateHistoryIssueCommand(int IssueId, string PerformedBy): INotification;
	
}
