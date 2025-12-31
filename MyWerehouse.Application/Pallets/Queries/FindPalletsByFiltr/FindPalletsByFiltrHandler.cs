using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;//odejście od CleanArchitecture dla wydajności, mniej kodu
using MediatR;
using Microsoft.EntityFrameworkCore;//odejście od CleanArchitecture dla wydajności, mniej kodu
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
/// QueryService zamiast Repository dla operacji Read.
/// Świadome odejście od czystego Repository Pattern na rzecz wydajności.
/// ProjectTo() generuje zoptymalizowane SQL SELECT tylko potrzebnych kolumn.
namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr
{
	public class FindPalletsByFiltrHandler:IRequestHandler<FindPalletsByFiltrQuery, List<PalletDTO>>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IMapper _mapper;
		public FindPalletsByFiltrHandler(IPalletRepo palletRepo,
			IMapper mapper)
		{
			_palletRepo = palletRepo;
			_mapper  = mapper;
		}
		public async Task<List<PalletDTO>> Handle (FindPalletsByFiltrQuery request, CancellationToken ct)
		{
			var pallets = _palletRepo.GetPalletsByFilter(request.Filter) ?? throw new PalletException("Brak palety/palet o zadanych parametrach");
			var palletDTO = await pallets.ProjectTo<PalletDTO>(_mapper.ConfigurationProvider).ToListAsync();
			return palletDTO;
		}
	}
}
