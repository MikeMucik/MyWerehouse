using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public class GetPalletToEditHandler(IPalletRepo palletRepo, IMapper mapper) : IRequestHandler<GetPalletToEditQuery, AppResult<EditPalletDTO>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<EditPalletDTO>> Handle(GetPalletToEditQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null) return AppResult<EditPalletDTO>.Fail("Brak palety do update", ErrorType.NotFound);

			var palletDTO = _mapper.Map<EditPalletDTO>(pallet);
			return AppResult<EditPalletDTO>.Success(palletDTO);
		}
	}
}
