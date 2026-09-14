using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Inventories.Queries.GetInventory
{
	public class GetInventoryHandler(IInventoryReadService inventoryReadService) : IRequestHandler<GetInventoryQuery, AppResult<InventoryDTO>>
	{
		public readonly IInventoryReadService _inventoryReadService = inventoryReadService;

		public async Task<AppResult<InventoryDTO>> Handle(GetInventoryQuery request, CancellationToken ct)
		{
			var inventory = await _inventoryReadService.GetInventory(request.ProductId, ct);
			if (inventory == null)
			{
				return AppResult<InventoryDTO>.Fail($"Inventory for product {request.ProductId} does not exist.");
			}			
			return AppResult<InventoryDTO>.Success(inventory);
		}
	}
}
