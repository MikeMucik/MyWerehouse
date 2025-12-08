using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Issues.Commands.CancelIssue
{
	public record CancelIssueCommand(int IssueId, string UserId):IRequest<IssueResult>;	
}
