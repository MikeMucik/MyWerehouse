using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;

namespace MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo
{
	public record GetListReversePickingToDoQuery(int PageSize, int PageNumber, DateOnly Start, DateOnly End)
		: IRequest<AppResult<PagedResult<ReversePickingDTO>>>;
	
}
