using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Picking.Queries.GetListPickingPallet;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Picking.Queries.GetListPickingPalletForOperator
{
	public class GetListPickingPalletHandler(IVirtualPalletRepo virtualPalletRepo)
		: IRequestHandler<GetListPickingPalletQuery, AppResult<PagedResult<PickingPalletWithLocationDTO>>>
	{
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;

		public async Task<AppResult<PagedResult<PickingPalletWithLocationDTO>>> Handle(GetListPickingPalletQuery request, CancellationToken ct)
		{			
			var palletsPicking = _virtualPalletRepo.GetVirtualPalletsByTimePickingTask(request.DateMovedStart, request.DateMovedEnd)
				.OrderBy(v=>v.LocationId)
				.Select(v=> new PickingPalletWithLocationDTO
				{
					PalletId = v.PalletId,
					PalletNumber = v.Pallet.PalletNumber,
					LocationId = v.LocationId,
					AddedToPicking = v.DateMoved,
					LocationName =
					v.Location.Bay + "-" +
					v.Location.Aisle + "-" +
					v.Location.Position + "-" +
					v.Location.Height
				});
			var query =await palletsPicking.ToPagedResultAsync(request.PageNumber, request.PageSize, ct);
			return AppResult<PagedResult<PickingPalletWithLocationDTO>>.Success(query);
		}
	}
}
