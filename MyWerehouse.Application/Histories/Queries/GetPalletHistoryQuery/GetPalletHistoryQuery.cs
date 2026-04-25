using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Histories.DTOs;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public record GetPalletHistoryQuery(Guid PalletId, int Page = 1, int PageSize = 50,
		DateTime? From = null, DateTime? To = null) : IRequest<PalletHistoryDTO>;
}
