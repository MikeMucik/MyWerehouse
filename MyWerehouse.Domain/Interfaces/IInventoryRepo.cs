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
		void IncreaseInventoryQuantity(int productId, int quantity);
		void DecreaseInventoryQuantity(int productId, int quantity);
		Inventory GetInventoryForProduct(int productId);		
		void UpdateInventory(int productId, int quantity);
		IQueryable<Inventory> GetAllInventory();
		bool HasStock(int productId, int quantity);
	}
}
//Exists(int productId)
//Sprawdza, czy dany produkt jest już ujęty w inwentarzu.

//RemoveProductFromInventory(int productId)
//Używane rzadko, ale przydatne np. gdy usuwasz produkt całkowicie.

//GetBelowMinimumStock()
//Pobiera produkty, których stan spadł poniżej ustalonego minimum – np. do alertów.


