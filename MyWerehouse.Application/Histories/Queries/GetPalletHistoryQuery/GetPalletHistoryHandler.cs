using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public class GetPalletHistoryHandler(IHistoryReadService historyReadService)
		: IRequestHandler<GetPalletHistoryQuery, AppResult<PalletHistoryDTO>>
	{
		private readonly IHistoryReadService _historyReadService = historyReadService;
		public async Task<AppResult<PalletHistoryDTO>> Handle(GetPalletHistoryQuery query, CancellationToken ct)
		{		
			var history = await _historyReadService.GetHistoryPallet(query.PalletNumber, query.PageNumber, query.PageSize, ct);

			if (history == null)
			{
				return AppResult<PalletHistoryDTO>.Fail("None history to show.");
			}
			return AppResult<PalletHistoryDTO>.Success(history);
		}
	}
}
