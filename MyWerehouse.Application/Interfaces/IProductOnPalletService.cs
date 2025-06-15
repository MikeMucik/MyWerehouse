using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductOnPalletService
	{
		void AddProductOnPalletReceipt(string palletId, int productId, int quantity);
		void AddProductToPalletPicking(string palletId, int productId, int quantity);
		void ProductToPalletPicking(string palletIdFrom, string palletIdTo,	int productId, int quantity);
	}
}
