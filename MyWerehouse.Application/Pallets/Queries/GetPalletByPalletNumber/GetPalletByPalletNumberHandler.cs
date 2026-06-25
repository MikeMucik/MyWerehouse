using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletByPalletNumber
{
	public class GetPalletByPalletNumberHandler(IPalletRepo palletRepo, IMapper mapper)
		: IRequestHandler<GetPalletByPalletNumberQuery, AppResult<PalletSimplyDTO>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IPalletRepo _palletRepo = palletRepo;
		
		public async Task<AppResult<PalletSimplyDTO>> Handle(GetPalletByPalletNumberQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByPalletNumberAsync(request.PalletNumber);
			if(pallet == null)
			{
				return AppResult<PalletSimplyDTO>.Fail("Paleta nie istnieje.", ErrorType.NotFound);
			}
			var palletDTO = _mapper.Map<PalletSimplyDTO>(pallet);
			return AppResult<PalletSimplyDTO>.Success(palletDTO);
		}
	}
}
