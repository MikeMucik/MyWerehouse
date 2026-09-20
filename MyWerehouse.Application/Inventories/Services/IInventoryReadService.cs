using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Inventories.Services
{
	public interface IInventoryReadService
	{
		Task<InventoryDTO?> GetInventory(Guid productId, CancellationToken ct);
		Task<PagedResult<InventoryDTO>> GetInventories(int pageNumber, int pageSize, CancellationToken ct);
	}
}
