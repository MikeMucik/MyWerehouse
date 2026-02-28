using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public record VerifyIssueToLoadCommand(Guid IssueId, string UserId) : IRequest<IssueResult>;
	
}
