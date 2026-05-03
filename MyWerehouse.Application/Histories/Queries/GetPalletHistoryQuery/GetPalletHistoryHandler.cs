using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public class GetPalletHistoryHandler(IPalletMovementRepo palletMovementRepo, IMapper mapper)
		:IRequestHandler<GetPalletHistoryQuery, AppResult<PagedResult<PalletHistoryDTO>>>
	{
		private readonly IPalletMovementRepo  _palletMovementRepo = palletMovementRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<PagedResult<PalletHistoryDTO>>> Handle(GetPalletHistoryQuery query , CancellationToken ct)
		{
			var history = _palletMovementRepo.GetDataByFilter(query.Filter,  query.PalletId);
			var result = await history.ToPagedResultAsync<PalletMovement, PalletHistoryDTO>(
				_mapper.ConfigurationProvider,
				query.Page, query.PageSize, ct);
			return AppResult<PagedResult<PalletHistoryDTO>>.Success(result);
		}
	}
}
