using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public class GetPalletToEditHandler(IPalletReadService palletReadService) : IRequestHandler<GetPalletToEditQuery, AppResult<ShowPalletToEditDTO>>
	{
		private readonly IPalletReadService _palletReadService = palletReadService;

		public async Task<AppResult<ShowPalletToEditDTO>> Handle(GetPalletToEditQuery request, CancellationToken ct)
		{
			var pallet = await _palletReadService.ShowPalletToEditAsync(request.PalletId, ct);
			if (pallet == null) return AppResult<ShowPalletToEditDTO>.Fail("No pallet was found to update.");
			return AppResult<ShowPalletToEditDTO>.Success(pallet);
		}
	}
}
