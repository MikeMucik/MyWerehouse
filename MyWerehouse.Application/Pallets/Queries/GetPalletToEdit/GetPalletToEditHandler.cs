using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public class GetPalletToEditHandler(IPalletRepo palletRepo, IMapper mapper) : IRequestHandler<GetPalletToEditQuery, UpdatePalletDTO>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<UpdatePalletDTO> Handle(GetPalletToEditQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			var palletDTO = _mapper.Map<UpdatePalletDTO>(pallet);
			return palletDTO;
		}
	}
}
