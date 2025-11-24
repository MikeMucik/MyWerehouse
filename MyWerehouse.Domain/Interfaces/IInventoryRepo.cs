using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IInventoryRepo
	{		
		void AddInventory (Inventory inventory);	
		Task<Inventory?> GetInventoryForProductAsync(int productId);
		Task<List<Inventory>> GetInventoriesForProductsAsync(List<int> productIds);//Do Dictionary
		IQueryable<Inventory> GetAllInventory();		
		Task <bool> HasStockAsync(int productId, int quantity);
		Task<int> GetAvailableQuantityAsync(int productId, DateOnly? bestBefore);
		Task<int> GetQuantityForProductAsync(int productId, DateOnly? bestBefore);
		Task<int> GetQuantityProductReservedForIssueAsync(int productId, DateOnly? bestBefore);
		Task<int> GetQuantityProductReservedForPickingAsync(int productId, DateOnly? bestBefore);
	}
}
//Exists(int productId)
//Sprawdza, czy dany produkt jest już ujęty w inwentarzu.

//RemoveProductFromInventory(int productId)
//Używane rzadko, ale przydatne np. gdy usuwasz produkt całkowicie.

//GetBelowMinimumStock()
//Pobiera produkty, których stan spadł poniżej ustalonego minimum – np. do alertów.


