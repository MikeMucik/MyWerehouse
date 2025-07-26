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
		Task AddProductToPalletAsync(ProductOnPallet product);		
		Task DeleteProductFromPalletAsync(string palletId, int productId);		
		Task UpdateProductQuantityAsync(string palletId, int productId, int newQuantity);
		IQueryable<ProductOnPallet> GetProductsOnPallets(string palletId);		
		Task<bool> ExistsAsync(string palletId, int productId);		
		Task ClearThePalletAsync(string palletId);		
		Task<int> GetQuantityAsync(string palletId, int productId);		
		Task IncreaseQuantityOnPalletAsync(string palletId, int productId, int quantity);		
		Task DecreaseQuantityOnPalletAsync(string palletId, int productId, int quantity);
		Task<List<QuantityLocation>> GetQuantityLocation(int productId);// do testu
	}
}

