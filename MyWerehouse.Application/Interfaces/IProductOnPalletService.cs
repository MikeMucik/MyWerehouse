using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ProductModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductOnPalletService
	{
		void AddProductOnPalletReceipt(string palletId, int productId, int quantity);//nie potrzebne
		void AddProductToPalletPicking(string palletId, int productId, int quantity);
		void ProductToPalletPicking(string palletIdFrom, string palletIdTo,	int productId, int quantity);
		Task<ProductQunatityLocationsDTO> GetQuantityLocationProduct(int productId);
		//stworzenie i realizacja listy palet kompletacyjnych
	}
}
