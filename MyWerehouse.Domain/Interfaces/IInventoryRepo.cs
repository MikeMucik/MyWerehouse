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
		Task<List<Inventory>> GetInventoriesForProductsAsync(List<Guid> productIds, CancellationToken ct);
		Task<bool> HasStockAsync(Guid productId, int quantity, CancellationToken ct);
		Task<int> GetAllocatableQuantityAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct);
	}
}
