using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class GetIssueByIdHandler : IRequestHandler<GetIssueByIdQuery, AppResult<IssueDTO>>
	{
		private readonly IIssueReadService _issueReadService;
		public GetIssueByIdHandler(IIssueReadService issueReadService)
		{
			_issueReadService = issueReadService;
		}
		public async Task<AppResult<IssueDTO>> Handle(GetIssueByIdQuery request, CancellationToken ct)
		{
			var issue = await _issueReadService.GetIssueById(request.IssueId, ct);
			if (issue == null)
				return AppResult<IssueDTO>.Fail("Issue was not found.");
			return AppResult<IssueDTO>.Success(issue);
		}
	}
}
