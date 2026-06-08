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
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr
{
	public class FindPalletsByFiltrHandler(IPalletRepo palletRepo,
		IMapper mapper) : IRequestHandler<FindPalletsByFiltrQuery, AppResult<PagedResult<PalletDTO>>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;
		public async Task<AppResult<PagedResult<PalletDTO>>> Handle(FindPalletsByFiltrQuery request, CancellationToken ct)
		{
			var pallets = _palletRepo.GetPalletsByFilter(request.Filter)
				.AsNoTracking();
			var palletsOrdered = pallets.OrderBy(p => p.Id);
			var result = await palletsOrdered
				.ProjectTo<PalletDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(request.CurrentPage,request.PageSize,ct);
			if (result.TotalCount == 0) return AppResult<PagedResult<PalletDTO>>.Fail("Brak palety/palet o zadanych parametrach", ErrorType.NotFound);
			return AppResult<PagedResult<PalletDTO>>.Success(result);
		}
	}
}