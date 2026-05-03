using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Domain.Histories.Filters;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public record GetPalletHistoryQuery(PalletMovementSearchFilter Filter, Guid PalletId, int Page = 1, int PageSize = 50,
		DateTime? From = null, DateTime? To = null) : IRequest<AppResult<PagedResult<PalletHistoryDTO>>>;
}
