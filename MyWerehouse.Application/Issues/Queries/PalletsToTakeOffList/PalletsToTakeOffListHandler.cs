using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletsToTakeOffListHandler(IIssueReadService issueReadService) : IRequestHandler<PalletsToTakeOffListQuery, AppResult<PagedResult<PalletWithLocationDTO>>>
	{		
		private readonly IIssueReadService _issueReadService = issueReadService;
		public async Task<AppResult<PagedResult<PalletWithLocationDTO>>> Handle(PalletsToTakeOffListQuery request, CancellationToken ct)
		{
			var dto = await _issueReadService.GetPalletToTakeOff(request.IssueId, request.PageNumber, request.PageSize, ct);
			return AppResult<PagedResult<PalletWithLocationDTO>>.Success(dto);
		}
	}
}
