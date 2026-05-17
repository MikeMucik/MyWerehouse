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
	public class GetPalletHistoryQuery
		//(HistoryPalletSearchFilter Filter, Guid PalletId, int Page = 1, int PageSize = 50)
		// : IRequest<AppResult<PagedResult<PalletHistoryDTO>>>
		 : IRequest<AppResult<PalletHistoryDTO>>
	{
		public HistoryPalletSearchFilter Filter { get; set; } = new();
		public Guid PalletId { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
	};
}
