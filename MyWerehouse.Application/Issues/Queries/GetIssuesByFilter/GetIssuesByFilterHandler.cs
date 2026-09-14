using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFilter
{
	public class GetIssuesByFilterHandler(IIssueReadService issueReadService) 
		: IRequestHandler<GetIssuesByFilterQuery, AppResult<PagedResult<IssueSimplyDTO>>>
	{
		private readonly IIssueReadService _issueReadService = issueReadService;
		public async Task<AppResult<PagedResult<IssueSimplyDTO>>> Handle(GetIssuesByFilterQuery request, CancellationToken ct)
		{
			var issues = await _issueReadService.GetIssueByFilter(request.Filter, request.CurrentPage, request.PageSize, ct);
						
			if (issues.TotalCount == 0) return AppResult<PagedResult<IssueSimplyDTO>>.Fail("No issues match the specified criteria.");
			return AppResult<PagedResult<IssueSimplyDTO>>.Success(issues);
		}
	}
}
