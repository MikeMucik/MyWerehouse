using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
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
			if (String.IsNullOrEmpty(query.PalletNumber))
			{
				return AppResult<PalletHistoryDTO>.Fail("Pallet number was not provided.", ErrorType.Validation);
			}
			var pallet = await _palletRepo.GetPalletByPalletNumberAsync(query.PalletNumber);
			if (pallet == null)
			{
				return AppResult<PalletHistoryDTO>.Fail($"Pallet {query.PalletNumber} does not exist.");
			}
			
			var history = await _palletMovementRepo.GetHistoryPallet(query.PalletNumber);

			var historyOrdered = history.OrderBy(x => x.MovementDate);

			var result = _mapper.Map<List<HistoryPalletDTO>>(historyOrdered);
						
			var historyForPallet = new PalletHistoryDTO
			{
				Id = pallet.Id,
				PalletNumber = pallet.PalletNumber,
				DateReceived = pallet.DateReceived,
				ReceiptId = pallet.Receipt?.Id,
				ReceiptNumber = pallet.Receipt?.ReceiptNumber,
				IssueId = pallet.Issue?.Id,
				IssueNumber = pallet.Issue?.IssueNumber,
				PalletMovementsDTO = result
			};
			return AppResult<PalletHistoryDTO>.Success(historyForPallet);
		}
	}
}
