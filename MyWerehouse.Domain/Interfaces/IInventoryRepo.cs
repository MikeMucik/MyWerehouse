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
		void AddInventory (Inventory inventory);	
		Task<Inventory?> GetInventoryForProductAsync(Guid productId);
		Task<List<Inventory>> GetInventoriesForProductsAsync(List<Guid> productIds);
		IQueryable<Inventory> GetAllInventory();		
		Task <bool> HasStockAsync(Guid productId, int quantity);
		Task<int> GetAvailableQuantityAsync(Guid productId, DateOnly? bestBefore);
		Task<int> GetQuantityForProductAsync(Guid productId, DateOnly? bestBefore);
		Task<int> GetQuantityProductReservedForIssueAsync(Guid productId, DateOnly? bestBefore);
		Task<int> GetQuantityProductReservedForPickingAsync(Guid productId, DateOnly? bestBefore);
	}
}



