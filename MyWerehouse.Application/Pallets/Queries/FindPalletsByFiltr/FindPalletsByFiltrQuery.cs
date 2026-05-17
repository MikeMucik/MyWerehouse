using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Filters;

namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr
{
	public class FindPalletsByFiltrQuery : IRequest<AppResult<PagedResult<PalletDTO>>>
	{
		public PalletSearchFilter Filter { get; set; } = new();
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
	};	
}
