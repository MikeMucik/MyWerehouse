using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.DeleteIssue
{
	public record DeleteIssueCommand(int IssueId, string UserId) : IRequest<IssueResult>;
	
}
