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
		Task AddInventoryAsync (int productId, int quantity);
		Task IncreaseInventoryQuantityAsync(int productId, int quantity);		
		Task DecreaseInventoryQuantityAsync(int productId, int quantity);		
		Task<Inventory?> GetInventoryForProductAsync(int productId);			
		Task UpdateInventoryAsync(int productId, int quantity);
		IQueryable<Inventory> GetAllInventory();		
		Task <bool> HasStockAsync(int productId, int quantity);
		 
	}
}
//Exists(int productId)
//Sprawdza, czy dany produkt jest już ujęty w inwentarzu.

//RemoveProductFromInventory(int productId)
//Używane rzadko, ale przydatne np. gdy usuwasz produkt całkowicie.

//GetBelowMinimumStock()
//Pobiera produkty, których stan spadł poniżej ustalonego minimum – np. do alertów.


