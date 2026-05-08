using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public record ModifyIssueCommand(ModifyIssueDTO DTO, DateTime DateToSend) : IRequest<AppResult<List<IssueResult>>>;	
}