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
		void DeleteProductFromPallet(string palletId, int productId);
		void UpdateProductQuantity(string palletId, int productId, int newQuantity);
		IQueryable<ProductOnPallet> GetProductsOnPallets(string palletId);
		bool Exists(string palletId, int productId);
		void ClearThePallet(string palletId);
		int GetQuantity(string palletId, int productId);
	}
}

//GetAllAsync() (opcjonalnie)
//Pobiera wszystkie wpisy – przydatne np. do eksportów, raportów.
