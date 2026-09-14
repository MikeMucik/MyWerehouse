using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.IssueProductsSummary
{
	public class IssueProductsSummaryHandler(IIssueReadService issueReadService) : IRequestHandler<IssueProductsSummaryQuery, AppResult<SummaryProductsIssueDTO>>
	{
		private readonly IIssueReadService _issueReadService = issueReadService;

		public async Task<AppResult<SummaryProductsIssueDTO>> Handle(IssueProductsSummaryQuery query, CancellationToken ct)
		{
			var dto = await _issueReadService.SummaryProductsIssue(query.IssueId, ct);
			
			if (dto == null)
			{
				return AppResult<SummaryProductsIssueDTO>.Fail($"Issue {query.IssueId} was not found.");
			}
			return AppResult<SummaryProductsIssueDTO>.Success(dto);
		}
	}
}
