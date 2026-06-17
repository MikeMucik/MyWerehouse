using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Queries.GetPallet;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletBySKU
{
	public class GetPalletByPalletNumberHandler(IPalletRepo palletRepo, IMapper mapper)
		: IRequestHandler<GetPalletByPalletNumberQuery, AppResult<PalletDTO>>
	{
		private readonly IMapper _mapper = mapper;
		private readonly IPalletRepo _palletRepo = palletRepo;
		
		public async Task<AppResult<PalletDTO>> Handle(GetPalletByPalletNumberQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByPalletNumberFullInfoAsync(request.palletNumber);
			if(pallet == null)
			{
				return AppResult<PalletDTO>.Fail("Paleta nie istnieje.", ErrorType.NotFound);
			}
			var palletDTO = _mapper.Map<PalletDTO>(pallet);
			return AppResult<PalletDTO>.Success(palletDTO);
		}
	}
}
