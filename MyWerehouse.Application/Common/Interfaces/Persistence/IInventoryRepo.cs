using MyWerehouse.Domain.Inventories.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IInventoryRepo
	{
		void AddInventory(Inventory inventory);
		Task<List<Inventory>> GetInventoriesForProductsAsync(List<Guid> productIds, CancellationToken ct);
		Task<bool> HasStockAsync(Guid productId, int quantity, CancellationToken ct);
		Task<int> GetAllocatableQuantityAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct);
	}
}
