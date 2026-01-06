using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Commands.AddPalletsToIssueByProduct
{
	public record AddPalletsToIssueByProductCommand(Issue Issue, IssueItemDTO Product) : IRequest<IssueResult>;
}
