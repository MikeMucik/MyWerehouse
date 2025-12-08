using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr
{
	public record GetIssuesByFiltrQuery(IssueReceiptSearchFilter Filtr):IRequest<List<IssueDTO>>;
	
}
