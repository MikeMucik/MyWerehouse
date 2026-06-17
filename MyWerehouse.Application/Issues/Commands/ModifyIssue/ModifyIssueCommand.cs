using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssueServices;

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public record ModifyIssueCommand(Guid Id, ModifyIssueDTO DTO, DateOnly DateToSend)
		: IRequest<AppResult<List<AssignProductToIssueResult>>>;	
}