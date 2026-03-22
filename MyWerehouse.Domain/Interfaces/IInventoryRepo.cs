using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Invetories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IInventoryRepo
	{		
		void AddInventory (Inventory inventory);	
		Task<Inventory?> GetInventoryForProductAsync(Guid productId);
		Task<List<Inventory>> GetInventoriesForProductsAsync(List<Guid> productIds);//Do Dictionary
		IQueryable<Inventory> GetAllInventory();		
		Task <bool> HasStockAsync(Guid productId, int quantity);
		Task<int> GetAvailableQuantityAsync(Guid productId, DateOnly? bestBefore);
		Task<int> GetQuantityForProductAsync(Guid productId, DateOnly? bestBefore);
		Task<int> GetQuantityProductReservedForIssueAsync(Guid productId, DateOnly? bestBefore);
		Task<int> GetQuantityProductReservedForPickingAsync(Guid productId, DateOnly? bestBefore);
	}
}
//Exists(int productId)
//Sprawdza, czy dany produkt jest już ujęty w inwentarzu.

//RemoveProductFromInventory(int productId)
//Używane rzadko, ale przydatne np. gdy usuwasz produkt całkowicie.

//GetBelowMinimumStock()
//Pobiera produkty, których stan spadł poniżej ustalonego minimum – np. do alertów.


