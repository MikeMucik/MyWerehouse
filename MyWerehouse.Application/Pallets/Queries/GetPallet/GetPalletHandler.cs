using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPallet
{
	public class GetPalletHandler(IPalletRepo palletRepo, IMapper mapper)
		:IRequestHandler<GetPalletQuery, AppResult<PalletDTO>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;
		public async Task<AppResult<PalletDTO>> Handle(GetPalletQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdFullInfoAsync(request.Id);
			var result = _mapper.Map<PalletDTO>(pallet);
			return AppResult<PalletDTO>.Success(result);
		}
	}
	
}
