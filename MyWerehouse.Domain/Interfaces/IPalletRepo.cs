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
		Pallet GetPalletById(string palletId);
		Pallet GetPalletWithProducts(string palletId);
		Pallet GetPalletWithHistory(string palletId);
		IEnumerable<Pallet> GetAvailablePallets(int productId, DateOnly BestBefore);
		IQueryable<Pallet> GetPalletsByBasedFilter(PalletSearchFilter filter);
		IQueryable<Pallet> GetPalletsByClientFilter(PalletSearchFilter filter);
		IQueryable<Pallet> GetPalletsByUser(PalletSearchFilter filter);
		void ClearPalletFromListReceipt(string palletId);
		void ClearPalletFromListIssue(string palletId);
		//TODO expand to methods
		void MarkPalletAsHold(string id);
		void MarkPalletAsAvailable(string id);
		void MarkPalletAsDamaged(string id);
		void MarkPalletAsLoaded(string id);
		//IQueryable<Location> GetLocationsFromFilteredPallets(PalletSearchFilter filter);
	}
}
