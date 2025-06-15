using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IProductOnPalletRepo
	{
		void AddProductToPallet(ProductOnPallet product);
		Task AddProductToPalletAsync(ProductOnPallet product);
		void DeleteProductFromPallet(string palletId, int productId);
		Task DeleteProductFromPalletAsync(string palletId, int productId);
		void UpdateProductQuantity(string palletId, int productId, int newQuantity);
		Task UpdateProductQuantityAsync(string palletId, int productId, int newQuantity);
		IQueryable<ProductOnPallet> GetProductsOnPallets(string palletId);
		bool Exists(string palletId, int productId);
		Task<bool> ExistsAsync(string palletId, int productId);
		void ClearThePallet(string palletId);
		Task ClearThePalletAsync(string palletId);
		int GetQuantity(string palletId, int productId);
		Task<int> GetQuantityAsync(string palletId, int productId);
		void IncreaseQuantityOnPallet(string palletId, int productId, int quantity);
		Task IncreaseQuantityOnPalletAsync(string palletId, int productId, int quantity);
		void DecreaseQuantityOnPallet(string palletId, int productId, int quantity);
		Task DecreaseQuantityOnPalletAsync(string palletId, int productId, int quantity);
	}
}

//GetAllAsync() (opcjonalnie)
//Pobiera wszystkie wpisy – przydatne np. do eksportów, raportów.
