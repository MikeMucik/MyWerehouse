using MediatR;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo
{
	public class GetReversePickingToDoHandler(IReversePickingRepo reversePickingRepo,
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		IReversePickingReadService reversePickingReadService) : IRequestHandler<GetReversePickingToDoQuery, AppResult<ReversePickingDetailsDTO>>
	{
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;	
		private readonly IReversePickingReadService _reversePickingReadService = reversePickingReadService;

		public async Task<AppResult<ReversePickingDetailsDTO>> Handle(GetReversePickingToDoQuery query, CancellationToken ct)
		{
			var reversePickingTask = await _reversePickingRepo.GetReversePickingAsync(query.ReversePickingTaskId, ct);
			if (reversePickingTask == null) return AppResult<ReversePickingDetailsDTO>.Fail("Reverse picking task was not found.");

			var pickingTask = reversePickingTask.PickingTask;
			var remainingQuantity = pickingTask.PickedQuantity;
			var product = await _productRepo.GetProductByIdAsync(pickingTask.ProductId, ct);
			if (product == null) return AppResult<ReversePickingDetailsDTO>.Fail($"Product {pickingTask.ProductId} does not exist.");
			if (product.CartonsPerPallet == 0) return AppResult<ReversePickingDetailsDTO>.Fail($"Product {pickingTask.ProductId} has no cartons-per-pallet value. Update the product.", ErrorType.Conflict);
			var sourcePallet = pickingTask.VirtualPallet?.Pallet;
			if (sourcePallet == null) return AppResult<ReversePickingDetailsDTO>.Fail("Source pallet does not exist.");
			// czy można dołączyć do palety z której pobierano
			bool addSource = false;
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				addSource = true;
			}
			//czy istnieje paleta/y do której można dodać
			var palletsFromBase = await _palletRepo.GetAvailablePalletsForReversePickingAsync(pickingTask.ProductId,
				reversePickingTask.BestBefore, sourcePallet.Id, product.CartonsPerPallet, ct);
			//lista palet do których dodamy
			bool canAddedtoExist = false;
			bool unpickComplete = false;
			var listPalletsToAdd = new List<Guid>();
			foreach (var pallet in palletsFromBase)
			{
				if (remainingQuantity <= 0) break;
				var palletLackQuantity = product.CartonsPerPallet - pallet.ProductsOnPallet.Single().Quantity;
				remainingQuantity -= palletLackQuantity;
				listPalletsToAdd.Add(pallet.Id);
				canAddedtoExist = true;
				if (remainingQuantity <= 0)
				{
					unpickComplete = true;
					break;
				}
			}
			var reverseDTO = await _reversePickingReadService.GetReversePicking(reversePickingTask.Id,	ct);
			if (reverseDTO == null)
			{
				return AppResult<ReversePickingDetailsDTO>.Fail("No elements to show.");
			}
			var response = new ReversePickingDetailsDTO
			{
				AddToNewPallet = true,
				CanReturnToSource = addSource,
				CanAddToExistingPallet = canAddedtoExist,//muszą być oba lub żadne
				ListPalletsToAdd = listPalletsToAdd,//muszą być oba lub żadne
				PickingPalletCompletlyUnpicking = unpickComplete,
				ReversePickingDTO = reverseDTO
			};			
			return AppResult<ReversePickingDetailsDTO>.Success(response);
		}
	}
}
