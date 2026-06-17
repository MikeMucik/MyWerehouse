using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssueServices;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public record CreateIssueCommand(CreateIssueDTO DTO, DateOnly SendDate)
		: IRequest<AppResult<List<AssignProductToIssueResult>>>;	
}
