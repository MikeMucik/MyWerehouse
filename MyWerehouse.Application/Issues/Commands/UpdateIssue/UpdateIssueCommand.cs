using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Issues.Commands.UpdateIssue
{
	public record UpdateIssueCommand(UpdateIssueDTO DTO, DateTime DateToSend) : IRequest<List<IssueResult>>;
	
}
