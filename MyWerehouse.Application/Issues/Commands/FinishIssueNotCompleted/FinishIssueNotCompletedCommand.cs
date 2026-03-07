using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted
{
	public record FinishIssueNotCompletedCommand(Guid IssueId, string UserId): IRequest<AppResult<Unit>>;
	
}
