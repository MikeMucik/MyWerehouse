using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pickings.Queries.GetListIssueToPickingTree
{
	public record GetListIssueToPickingQuery(DateOnly DateIssueStart, DateOnly DateIssueEnd)
		:IRequest<AppResult<List<PickingGuideLineDTO>>>;	
}
