using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading
{
	public record ChangePalletInIssueCommand(int IssueId, string OldPalletId, string NewPalletId, string UserId):IRequest<IssueResult>;
}
