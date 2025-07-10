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
		Task<string> AddPalletAsync(Pallet pallet);
		void DeletePallet(string palletId);
		Task DeletePalletAsync(string palletId);
		Pallet? GetPalletById(string palletId);
		Task<Pallet?> GetPalletByIdAsync(string palletId);
		IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly minBestBefore);
		IQueryable<Pallet> GetPalletsByBasedFilter(PalletSearchFilter filter);
		IQueryable<Pallet> GetPalletsByClientFilter(PalletSearchFilter filter);
		IQueryable<Pallet> GetPalletsByUser(PalletSearchFilter filter);
		void ClearPalletFromListReceipt(string palletId);
		void ClearPalletFromListIssue(string palletId);
		void ChangePalletStatus(string palletId, PalletStatus palletStatus);
		string GetNextPalletId();
		Task<string> GetNextPalletIdAsync();
		Pallet GetPalletByLocation(int locationId); //numer lokacji
		Task<Pallet> GetPalletByLocationAsync(int locationId); //numer lokacji
															   
		//TODO expand to methods
	   //void MarkPalletAsHold(string id);
	   //void MarkPalletAsAvailable(string id);
	   //void MarkPalletAsDamaged(string id);
	   //void MarkPalletAsLoaded(string id);

		//IQueryable<Location> GetLocationsFromFilteredPallets(PalletSearchFilter filter);
	}
}
