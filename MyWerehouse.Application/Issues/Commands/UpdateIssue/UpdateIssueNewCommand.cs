using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Commands.UpdateIssue
{
	public record UpdateIssueNewCommand(UpdateIssueDTO DTO, DateTime DateToSend) : IRequest<List<IssueResult>>;	
}
