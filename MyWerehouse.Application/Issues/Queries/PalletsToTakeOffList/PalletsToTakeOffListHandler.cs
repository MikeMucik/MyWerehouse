using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletsToTakeOffListHandler( WerehouseDbContext werehouseDbContext) : IRequestHandler<PalletsToTakeOffListQuery, AppResult<PagedResult<PalletWithLocationDTO>>>
	{		
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		public async Task<AppResult<PagedResult<PalletWithLocationDTO>>> Handle(PalletsToTakeOffListQuery request, CancellationToken ct)
		{
			var query = _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(i => i.IssueId == request.IssueId)
				.OrderBy(l => l.Location.Bay)
					.ThenBy(l => l.Location.Aisle)
						.ThenBy(l => l.Location.Position)
							.ThenBy(l => l.Location.Height)
				.Select(p => new PalletWithLocationDTO
				{
					PalletId = p.Id,
					PalletNumber = p.PalletNumber,
					LocationId = p.LocationId,
					Status = p.Status,
					LocationName =
					p.Location.Bay + "-" +
					p.Location.Aisle + "-" +
					p.Location.Position + "-" +
					p.Location.Height
				});
			var dto = await query
				.ToPagedResultAsync(request.PageNumber, request.PageSize, ct);
			return AppResult<PagedResult<PalletWithLocationDTO>>.Success(dto);
		}
	}
}
