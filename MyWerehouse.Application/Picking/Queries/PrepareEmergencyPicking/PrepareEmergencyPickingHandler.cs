using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Picking.Queries.PrepareEmergencyPicking
{
	public class PrepareEmergencyPickingHandler(IPalletRepo palletRepo, 
		IPickingReadService pickingReadService
	) : IRequestHandler<PrepareEmergencyPickingQuery, AppResult<PrepareCorrectedPickingResult>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingReadService _pickingReadService = pickingReadService;

		public async Task<AppResult<PrepareCorrectedPickingResult>> Handle(PrepareEmergencyPickingQuery request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId, ct);
			if (pallet == null)
			{
				return AppResult<PrepareCorrectedPickingResult>.Fail($"Pallet was not found in warehouse stock.");
			}
			var product = pallet.EnsureCanBeUsedForPicking();//to jest walidacja palety źródło
			var properPickingTask = await _pickingReadService.GetProperpickingTask(product.ProductId, request.Start, request.End, ct);
			var result = PrepareCorrectedPickingResult.RequiresOrder(
				productInfo: $"{product.PalletId} : {product.Quantity}",
				issueOptions: properPickingTask,
				message: "Provide the issue number to continue.");
			return AppResult<PrepareCorrectedPickingResult>.Success(result);
		}
	}
}
