using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Inventories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IInventoryRepo
	{
		void AddInventory(Inventory inventory);
		Task<Inventory?> GetInventoryForProductAsync(Guid productId, CancellationToken ct);
		Task<List<Inventory>> GetInventoriesForProductsAsync(List<Guid> productIds, CancellationToken ct);
		IQueryable<Inventory> GetAllInventory();
		Task<bool> HasStockAsync(Guid productId, int quantity, CancellationToken ct);
		Task<int> GetAllocatableQuantityAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct);
	}
}
