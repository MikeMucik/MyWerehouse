using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Queries.LoadingIssueList
{
	public record LoadingIssueListQuery(Guid IssueId, string UserId): IRequest<ListPalletsToLoadDTO>;
	
}
