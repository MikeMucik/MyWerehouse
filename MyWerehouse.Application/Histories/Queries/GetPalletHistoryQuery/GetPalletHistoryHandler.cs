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
	public class GetPalletHistoryHandler(IHistoryPalletRepo palletMovementRepo, IMapper mapper, IPalletRepo palletRepo)
		: IRequestHandler<GetPalletHistoryQuery, AppResult<PalletHistoryDTO>>
	{
		private readonly IHistoryPalletRepo _palletMovementRepo = palletMovementRepo;
		private readonly IMapper _mapper = mapper;
		private readonly IPalletRepo _palletRepo = palletRepo;
		public async Task<AppResult<PalletHistoryDTO>> Handle(GetPalletHistoryQuery query, CancellationToken ct)
		{
			if (query.PalletId == null || query.PalletId == Guid.Empty)
			{
				return AppResult<PalletHistoryDTO>.Fail("Nie podano numeru palety");
			}
			var pallet = await _palletRepo.GetPalletByIdAsync(query.PalletId);
			var history = _palletMovementRepo.GetDataByFilter(query.Filter, query.PalletId)
				.AsNoTracking();
			var historyOrdered = history.OrderBy(x => x.MovementDate);
			var historyDTO = await historyOrdered
				.ProjectTo<HistoryPalletDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(query.Page, query.PageSize, ct);
			var r = new PalletHistoryDTO
			{
				Id = query.PalletId,
				PalletNumber = pallet.PalletNumber,
				DateReceived = pallet.DateReceived,
				ReceiptId = pallet.Receipt?.Id,
				ReceiptNumber = pallet.Receipt?.ReceiptNumber,
				IssueId = pallet.Issue?.Id,
				IssueNumber = pallet.Issue?.IssueNumber,
				PalletMovementsDTO = historyDTO
			};
			return AppResult<PalletHistoryDTO>.Success(r);
		}
	}
}