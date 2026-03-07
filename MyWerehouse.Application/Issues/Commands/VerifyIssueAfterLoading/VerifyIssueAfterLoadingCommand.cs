using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading
{
	public record VerifyIssueAfterLoadingCommand(Guid IssueId, string VerifiedBy) : IRequest<AppResult<Unit>>;
	
}
