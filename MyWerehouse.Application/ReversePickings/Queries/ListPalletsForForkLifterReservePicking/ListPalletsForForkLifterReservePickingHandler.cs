using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking
{
	public class ListPalletsForForkLifterReservePickingHandler(
		IReversePickingReadService reversePickingReadService,
		//IReversePickingRepo reversePickingRepo,
		//IPalletRepo palletRepo,
		IDateTimeProvider dateTimeProvider)
		: IRequestHandler<ListPalletsForForkLifterReservePickingQuery, AppResult<List<ReversePickingPalletWithLocationDTO>>>
	{
		private readonly IReversePickingReadService _reversePickingReadService = reversePickingReadService;
		//private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		//private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<List<ReversePickingPalletWithLocationDTO>>> Handle(ListPalletsForForkLifterReservePickingQuery query, CancellationToken ct)
		{
			//var list = new List<ReversePickingPalletWithLocationDTO>();
			var dateStart = query.Start ?? _dateTimeProvider.Today.AddDays(-1);
			var dateEnd = query.End ?? _dateTimeProvider.Today;

			var palletsWithLocation = await _reversePickingReadService.GetListPalletsToReversePicking(dateStart, dateEnd, ct);
			if (palletsWithLocation.Count == 0)
			{
				return AppResult<List<ReversePickingPalletWithLocationDTO>>.Fail("No pallets to display.");
			}
			return AppResult<List<ReversePickingPalletWithLocationDTO>>.Success(palletsWithLocation);
			//var palletsIds = await _reversePickingRepo.GetPalletsIdsByDate(dateStart, dateEnd, ct);
			//if (palletsIds.Count == 0)
			//{
			//	return AppResult<List<ReversePickingPalletWithLocationDTO>>.Fail("No pallets to display.");
			//}
			//foreach (var id in palletsIds)
			//{
			//	var pallet = await _palletRepo.GetPalletByIdAsync(id, ct);
			//	if (pallet == null)
			//		return AppResult<List<ReversePickingPalletWithLocationDTO>>.Fail($"Pallet {id} was not found.");
			//	var locationName = pallet.Location;
			//	var fullLocation = locationName.ToSnapshot();
			//	var item = new ReversePickingPalletWithLocationDTO
			//	{
			//		PalletId = id,
			//		PalletNumber = pallet.PalletNumber,
			//		LocationId = pallet.LocationId,
			//		LocationName = fullLocation,
			//		Status = pallet.Status,
			//	};
			//	list.Add(item);
			//}
			//return AppResult<List<ReversePickingPalletWithLocationDTO>>.Success(list);
		}
	}
}
