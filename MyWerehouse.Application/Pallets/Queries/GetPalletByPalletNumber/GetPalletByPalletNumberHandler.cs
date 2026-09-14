using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletByPalletNumber
{
	public class GetPalletByPalletNumberHandler(IPalletReadService palletReadService)
		: IRequestHandler<GetPalletByPalletNumberQuery, AppResult<PalletSimplyDTO>>
	{
		private readonly IPalletReadService _palletReadService = palletReadService;

		public async Task<AppResult<PalletSimplyDTO>> Handle(GetPalletByPalletNumberQuery request, CancellationToken ct)
		{
			var pallet = await _palletReadService.GetPalletByPalletNumberAsync(request.PalletNumber, ct);
			if(pallet == null)
			{
				return AppResult<PalletSimplyDTO>.Fail("Pallet does not exist.");
			}
			return AppResult<PalletSimplyDTO>.Success(pallet);
		}
	}
}
