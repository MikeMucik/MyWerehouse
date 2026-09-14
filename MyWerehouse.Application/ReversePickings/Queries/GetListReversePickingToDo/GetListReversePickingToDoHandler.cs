using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ReversePickings.DTOs;

namespace MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo
{
	public class GetListReversePickingToDoHandler(IReversePickingReadService reversePickingReadService) : IRequestHandler<GetListReversePickingToDoQuery, AppResult<PagedResult<ReversePickingDTO>>>
	{
		private readonly IReversePickingReadService _reversePickingReadService = reversePickingReadService;

		public async Task<AppResult<PagedResult<ReversePickingDTO>>> Handle (GetListReversePickingToDoQuery query, CancellationToken ct)
		{
			var result = await _reversePickingReadService.GetReversePickingsInDates(query.Start, query.End, query.PageNumber, query.PageSize, ct);
			if(result.TotalCount == 0)return AppResult<PagedResult<ReversePickingDTO>>.Fail("No reverse picking tasks were found.");
			return AppResult<PagedResult<ReversePickingDTO>>.Success(result); 
		}
	}
}
