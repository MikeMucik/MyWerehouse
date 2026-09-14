using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Picking.Queries.GetListPickingPallet;

namespace MyWerehouse.Application.Picking.Queries.GetListPickingPalletForOperator
{
	public class GetListPickingPalletHandler(IPickingReadService pickingReadService)
		: IRequestHandler<GetListPickingPalletQuery, AppResult<PagedResult<PickingPalletWithLocationDTO>>>
	{
		private readonly IPickingReadService _pickingReadService = pickingReadService;

		public async Task<AppResult<PagedResult<PickingPalletWithLocationDTO>>> Handle(GetListPickingPalletQuery request, CancellationToken ct)
		{
			var query = await _pickingReadService.GetSourcePalletList(request.DateMovedStart, request.DateMovedEnd, request.PageNumber, request.PageSize, ct);
			return AppResult<PagedResult<PickingPalletWithLocationDTO>>.Success(query);
		}
	}
}
