using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListPickingPalletForOperator
{
	public class GetListPickingPalletHandler(IVirtualPalletRepo virtualPalletRepo,
		ILocationRepo locationRepo) : IRequestHandler<GetListPickingPalletQuery, AppResult<List<PickingPalletWithLocationDTO>>>
	{
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<AppResult<List<PickingPalletWithLocationDTO>>> Handle(GetListPickingPalletQuery request, CancellationToken ct)
		{
			var pickingPallets = new List<PickingPalletWithLocationDTO>();
			var palletsToPicking = await _virtualPalletRepo.GetVirtualPalletsByTimePickingTaskAsync(request.DateMovedStart, request.DateMovedEnd);
			foreach (var pallet in palletsToPicking)
			{
				var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
				if (locationName == null) return AppResult<List<PickingPalletWithLocationDTO>>.Fail($"Lokalizacja o numerze {pallet.LocationId} nie została znaleziona", ErrorType.NotFound);
				var addedToPicking = pallet.DateMoved;//??
				var palletInWarehouseDTO = new PickingPalletWithLocationDTO
				{
					PalletId = pallet.PalletId,
					PalletNumber = pallet.Pallet.PalletNumber,
					LocationName = locationName.ToSnapshot(),
					AddedToPicking = addedToPicking
				};
				pickingPallets.Add(palletInWarehouseDTO);
			}
		//await	pickingPallets.ToPagedResultAsync(1,1,ct);
			return AppResult<List<PickingPalletWithLocationDTO>>.Success(pickingPallets);
		}
	}
}
