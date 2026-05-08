using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;

namespace MyWerehouse.Application.Picking.Queries.GetListPickingPallet
{
	public record GetListPickingPalletQuery(DateOnly DateMovedStart, DateOnly DateMovedEnd, int PageNumber, int PageSize)
		: IRequest<AppResult<PagedResult<PickingPalletWithLocationDTO>>>;	
}
