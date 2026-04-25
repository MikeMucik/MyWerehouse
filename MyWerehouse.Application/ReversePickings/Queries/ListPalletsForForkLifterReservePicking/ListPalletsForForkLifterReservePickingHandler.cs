using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsToReservePicking
{
	public class ListPalletsForForkLifterReservePickingHandler(IReversePickingRepo reversePickingRepo,
		IPalletRepo palletRepo)
		: IRequestHandler<ListPalletsForForkLifterReservePickingQuery, AppResult<List<PalletWithLocationDTO>>>
	{
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<List<PalletWithLocationDTO>>> Handle(ListPalletsForForkLifterReservePickingQuery query, CancellationToken ct)
		{
			var list = new List<PalletWithLocationDTO>();
			var palletsIds = await _reversePickingRepo.GetPalletsIdsByDate(query.Start, query.End);
			if (palletsIds.Count == 0)
			{
				return AppResult<List<PalletWithLocationDTO>>.Fail("Brak palet do wyświetlenia.");
			}
			foreach (var id in palletsIds)
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(id);
				if (pallet == null)
					return AppResult<List<PalletWithLocationDTO>>.Fail($"Brak palety w systemie {id}.");	//		
				var locationName = pallet.Location;
				var fullLocation = locationName.ToSnopShot();
				var item = new PalletWithLocationDTO
				{
					PalletId = id,
					PalletNumber = pallet.PalletNumber,
					LocationId = pallet.LocationId,
					LocationName = fullLocation,
				};
				list.Add(item);
			}
			return AppResult<List<PalletWithLocationDTO>>.Success(list);
		}
	}
}
