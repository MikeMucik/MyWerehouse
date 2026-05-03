using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr
{
	public record GetIssuesByFiltrQuery(IssueReceiptSearchFilter Filtr, int CurrentPage, int PageSize)
		:IRequest<AppResult<PagedResult<IssueDTO>>>;	
}
