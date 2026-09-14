using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Pallets.Queries.GetPallet
{
	public class GetPalletHandler(IPalletReadService palletReadService)
		:IRequestHandler<GetPalletQuery, AppResult<PalletDTO>>
	{
		private readonly IPalletReadService _palletReadService = palletReadService;
		public async Task<AppResult<PalletDTO>> Handle(GetPalletQuery request, CancellationToken ct)
		{			
			var pallet = await _palletReadService.GetPalletByIdFullInfoAsync(request.Id, ct);
			if (pallet == null)
			{
				return AppResult<PalletDTO>.Fail($"Pallet {request.Id} does not exist.");
			}
			return AppResult<PalletDTO>.Success(pallet);
		}
	}
}
