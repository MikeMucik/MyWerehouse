using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;

namespace MyWerehouse.Application.Pickings.Queries.GetListToPickingFlat
{//Product quantities per allocation (picking task)
 //Client -> issue -> product -> quantity, as a flat list
	public class GetListToPickingHandler(IPickingReadService pickingReadService) : IRequestHandler<GetListToPickingQuery, AppResult<List<ProductToIssueDTO>>>
	{
		private readonly IPickingReadService _pickingReadService = pickingReadService;

		public async Task<AppResult<List<ProductToIssueDTO>>> Handle(GetListToPickingQuery request, CancellationToken ct)
		{
			var data = await _pickingReadService.GetProductToIssueList(request.DateIssueStart, request.DateIssueEnd, ct);

			if (data.Count == 0)
			{
				return AppResult<List<ProductToIssueDTO>>.Fail("No picking items to display.");
			}			
			return AppResult<List<ProductToIssueDTO>>.Success(data);
		}
	}
}
