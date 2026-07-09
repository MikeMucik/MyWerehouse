using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Histories.DTOs;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public class GetPalletHistoryQuery : IRequest<AppResult<PalletHistoryDTO>>
	{			
		public required string PalletNumber { get; set; }
		//public int Page { get; set; }
		//public int PageSize { get; set; }
	};
}
