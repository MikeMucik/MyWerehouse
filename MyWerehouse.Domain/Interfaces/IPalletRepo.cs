using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPalletRepo
	{
		string AddPallet(Pallet pallet);
		void DeletePallet(string palletId);
		void UpdatePallet(Pallet pallet);
		Pallet GetPalletWithProducts(string palletId);
		Pallet GetPalletWithHistory(string palletId);
		IQueryable<Pallet> GetPalletsByBasedFilter(PalletSearchFilter filter);
		IQueryable<Pallet> GetPalletsByClientFilter(PalletSearchFilter filter);
		IQueryable<Pallet> GetPalletsByClientUser(PalletSearchFilter filter);
		//IQueryable<Pallet> GetAllPallets();
		//IQueryable<Pallet> FindPalletsByProductId(int productId);
		//IQueryable<Pallet> FindPalletsInLocation(int locationId);
		//IEnumerable<Pallet> FindPalletsByProductIdAndBestBefore(int productId);
	}
}
