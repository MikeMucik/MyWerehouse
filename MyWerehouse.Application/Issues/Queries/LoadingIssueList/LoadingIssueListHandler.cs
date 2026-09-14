using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.LoadingIssueList
{
	public class LoadingIssueListHandler(IIssueReadService issueReadService) : IRequestHandler<LoadingIssueListQuery, AppResult<ListPalletsToLoadDTO>>
	{
		private readonly IIssueReadService _issueReadService = issueReadService;

		public async Task<AppResult<ListPalletsToLoadDTO>> Handle(LoadingIssueListQuery request, CancellationToken ct)
		{
			var dto = await _issueReadService.ListPalletsToLoad(request.IssueId , ct);
			if (dto == null)
			{
				return AppResult<ListPalletsToLoadDTO>.Fail($"Issue {request.IssueId} was not found.");
			}
			return AppResult<ListPalletsToLoadDTO>.Success(dto);
		}
	}
}
