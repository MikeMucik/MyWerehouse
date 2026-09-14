using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Histories.DTOs;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public record GetPalletHistoryQuery(string PalletNumber,int PageNumber, int PageSize ) : IRequest<AppResult<PalletHistoryDTO>>;
}
