using MediatR;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking
{
	public class ListPalletsForForkLifterReservePickingHandler(
		IReversePickingReadService reversePickingReadService,
		IDateTimeProvider dateTimeProvider)
		: IRequestHandler<ListPalletsForForkLifterReservePickingQuery, AppResult<List<ReversePickingPalletWithLocationDTO>>>
	{
		private readonly IReversePickingReadService _reversePickingReadService = reversePickingReadService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<List<ReversePickingPalletWithLocationDTO>>> Handle(ListPalletsForForkLifterReservePickingQuery query, CancellationToken ct)
		{
			var dateStart = query.Start ?? _dateTimeProvider.Today.AddDays(-1);
			var dateEnd = query.End ?? _dateTimeProvider.Today;

			var palletsWithLocation = await _reversePickingReadService.GetListPalletsToReversePicking(dateStart, dateEnd, ct);
			if (palletsWithLocation.Count == 0)
			{
				return AppResult<List<ReversePickingPalletWithLocationDTO>>.Fail("No pallets to display.");
			}
			return AppResult<List<ReversePickingPalletWithLocationDTO>>.Success(palletsWithLocation);
		}
	}
}
