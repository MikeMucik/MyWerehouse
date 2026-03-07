using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet
{
	public class GetListPickingPalletHandler(IPickingPalletRepo pickingPalletRepo,
		ILocationRepo locationRepo) : IRequestHandler<GetListPickingPalletQuery, AppResult<List<PickingPalletWithLocationDTO>>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<AppResult<List<PickingPalletWithLocationDTO>>> Handle(GetListPickingPalletQuery request, CancellationToken ct)
		{
			var pickingPallets = new List<PickingPalletWithLocationDTO>();
			var palletPicking = await _pickingPalletRepo.GetVirtualPalletsByTimePickingTaskAsync(request.DateMovedStart, request.DateMovedEnd);
			foreach (var pallet in palletPicking)
			{
				var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);//?? throw new NotFoundLocationException (pallet.LocationId);
				if (locationName == null) return AppResult < List < PickingPalletWithLocationDTO >>.Fail($"Lokalizacja o numerze {pallet.LocationId} nie została znaleziona", ErrorType.NotFound);
				var addedToPicking = pallet.DateMoved; ;
				var palletInWarehouseDTO = new PickingPalletWithLocationDTO
				{
					PalletId = pallet.PalletId,
					LocationName = locationName.Bay + " " + locationName.Aisle + " " + locationName.Position + " " + locationName.Height,
					AddedToPicking = addedToPicking
				};
				pickingPallets.Add(palletInWarehouseDTO);
			}
			return AppResult<List<PickingPalletWithLocationDTO>>.Success(pickingPallets);
		}
	}
}
