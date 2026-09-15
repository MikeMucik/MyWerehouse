using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	//Product quantity list; for now, just a few reports, so no additional repository etc.
	//List showing the quantity of a specific item for a specific order (tree structure)
	public class GetListIssueToPickingHandler(IPickingReadService pickingReadService
		) : IRequestHandler<GetListIssueToPickingQuery, AppResult<List<PickingGuideLineDTO>>>
	{

		private readonly IPickingReadService _pickingReadService = pickingReadService;

		public async Task<AppResult<List<PickingGuideLineDTO>>> Handle(GetListIssueToPickingQuery request, CancellationToken ct)
		{
			var data = await _pickingReadService.GetPickingTaskFlat(request.DateIssueStart, request.DateIssueEnd, ct);
			if (data.Count == 0)
			{
				return AppResult<List<PickingGuideLineDTO>>.Fail("No picking items to display.");
			}
			return AppResult<List<PickingGuideLineDTO>>.Success(data);
		}
	}
}
