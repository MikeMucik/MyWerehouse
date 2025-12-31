using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet
{
	public class GetListPickingPalletHandler:IRequestHandler<GetListPickingPalletQuery, List<PickingPalletWithLocationDTO>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly ILocationRepo _locationRepo;
		public GetListPickingPalletHandler(IPickingPalletRepo pickingPalletRepo,
			ILocationRepo locationRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_locationRepo = locationRepo;
		}
		public async Task<List<PickingPalletWithLocationDTO>> Handle (GetListPickingPalletQuery request, CancellationToken ct)
		{
			var pickingPallets = new List<PickingPalletWithLocationDTO>();
			var palletPicking = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(request.DateMovedStart, request.DateMovedEnd);
			foreach (var pallet in palletPicking)
			{
				var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
				if (locationName == null) throw new InvalidDataException($"Brak lokalizacji {pallet.LocationId} w magazynie");//It's shouldn't heppend
																															  //var addedToPicking = await _pickingPalletRepo.TakeDateAddedToPickingAsync(pallet.Id);
				var addedToPicking = pallet.DateMoved;
				var palletInWarehouseDTO = new PickingPalletWithLocationDTO
				{
					PalletId = pallet.PalletId,
					LocationName = locationName.Bay + " " + locationName.Aisle + " " + locationName.Position + " " + locationName.Height,
					AddedToPicking = addedToPicking
				};
				pickingPallets.Add(palletInWarehouseDTO);
			}
			return pickingPallets;
		}
	}
}
