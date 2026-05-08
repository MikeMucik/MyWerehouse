using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery
{
	public class GetPalletHistoryHandler(IHistoryPalletRepo palletMovementRepo, IMapper mapper)
		:IRequestHandler<GetPalletHistoryQuery, AppResult<PagedResult<PalletHistoryDTO>>>
	{
		private readonly IHistoryPalletRepo  _palletMovementRepo = palletMovementRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<PagedResult<PalletHistoryDTO>>> Handle(GetPalletHistoryQuery query , CancellationToken ct)
		{
			var history = _palletMovementRepo.GetDataByFilter(query.Filter,  query.PalletId)
				.AsNoTracking();
			var historyOrdered = history.OrderBy(x => x.MovementDate);
			var result = await historyOrdered
				.ProjectTo<PalletHistoryDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(query.Page, query.PageSize, ct);
			return AppResult<PagedResult<PalletHistoryDTO>>.Success(result);
		}
	}
}