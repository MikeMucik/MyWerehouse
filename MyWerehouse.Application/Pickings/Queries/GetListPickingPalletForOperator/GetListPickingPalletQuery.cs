using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.Queries.GetListPickingPalletForOperator;

namespace MyWerehouse.Application.Pickings.Queries.GetListPickingPallet
{
	public record GetListPickingPalletQuery(DateOnly DateMovedStart, DateOnly DateMovedEnd, int PageNumber, int PageSize)
		: IRequest<AppResult<PagedResult<PickingPalletWithLocationDTO>>>;	
}
