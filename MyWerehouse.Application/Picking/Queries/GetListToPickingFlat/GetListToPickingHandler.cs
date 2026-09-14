using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Picking.Queries.GetListToPickingFlat
{//Lista ile danego towaru dla danej alokacji Product's list by pickingTasks
 //klient -> zamówienie -> produkt -> ilośc -  płasko
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
