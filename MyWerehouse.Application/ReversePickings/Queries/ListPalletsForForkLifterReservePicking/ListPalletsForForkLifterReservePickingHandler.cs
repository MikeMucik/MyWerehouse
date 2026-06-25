using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking
{
	public class ListPalletsForForkLifterReservePickingHandler(IReversePickingRepo reversePickingRepo,
		IPalletRepo palletRepo)
		: IRequestHandler<ListPalletsForForkLifterReservePickingQuery, AppResult<List<PickingPalletWithLocationDTO>>>
	{
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<List<PickingPalletWithLocationDTO>>> Handle(ListPalletsForForkLifterReservePickingQuery query, CancellationToken ct)
		{
			var list = new List<PickingPalletWithLocationDTO>();
			var dateStart = query.Start ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
			var dateEnd = query.End ?? DateOnly.FromDateTime(DateTime.UtcNow);

			var palletsIds = await _reversePickingRepo.GetPalletsIdsByDate(dateStart, dateEnd);
			if (palletsIds.Count == 0)
			{
				return AppResult<List<PickingPalletWithLocationDTO>>.Fail("Brak palet do wyświetlenia.");
			}
			foreach (var id in palletsIds)
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(id);
				if (pallet == null)
					return AppResult<List<PickingPalletWithLocationDTO>>.Fail($"Brak palety w systemie {id}.");	//		
				var locationName = pallet.Location;
				var fullLocation = locationName.ToSnapshot();
				var item = new PickingPalletWithLocationDTO
				{
					PalletId = id,
					PalletNumber = pallet.PalletNumber,
					LocationId = pallet.LocationId,
					LocationName = fullLocation,
				};
				list.Add(item);
			}
			return AppResult<List<PickingPalletWithLocationDTO>>.Success(list);
		}
	}
}
