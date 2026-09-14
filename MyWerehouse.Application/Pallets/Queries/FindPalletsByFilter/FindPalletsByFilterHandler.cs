using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFilter
{
	public class FindPalletsByFilterHandler(
		IPalletReadService palletReadService) : IRequestHandler<FindPalletsByFilterQuery, AppResult<PagedResult<PalletSimplyDTO>>>
	{
		private readonly IPalletReadService _palletReadService = palletReadService;
		public async Task<AppResult<PagedResult<PalletSimplyDTO>>> Handle(FindPalletsByFilterQuery request, CancellationToken ct)
		{
			var result = await _palletReadService.GetPalletsByFilterAsync(request.Filter, request.CurrentPage, request.PageSize, ct);

			if (result.TotalCount == 0) return AppResult<PagedResult<PalletSimplyDTO>>.Fail("No pallets match the specified criteria.");
			return AppResult<PagedResult<PalletSimplyDTO>>.Success(result);
		}
	}
}
