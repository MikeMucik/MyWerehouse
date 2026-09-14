using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Inventories.Queries.GetInventories
{
	public class GetInventoriesHandler(IInventoryReadService inventoryReadService) : IRequestHandler<GetInventoriesQuery, AppResult< PagedResult<InventoryDTO>>>
	{
		private readonly IInventoryReadService _inventoryReadService = inventoryReadService;

		public async Task<AppResult<PagedResult<InventoryDTO>>> Handle(GetInventoriesQuery request, CancellationToken ct)
		{
			var inventories = await _inventoryReadService.GetInventories(request.PageNumber, request.PageSize, ct);
			return AppResult<PagedResult<InventoryDTO>>.Success(inventories);		
		}
	}
}
