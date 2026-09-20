using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pickings.Queries.GetListToPickingFlat
{
	public record GetListToPickingQuery(DateOnly DateIssueStart, DateOnly DateIssueEnd)
		:IRequest<AppResult<List<ProductToIssueDTO>>>;	
}
