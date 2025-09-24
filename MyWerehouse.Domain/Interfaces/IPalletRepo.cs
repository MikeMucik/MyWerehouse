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
		Task<string> AddPalletAsync(Pallet pallet);		
		Task DeletePalletAsync(string palletId);		
		Task<Pallet> GetPalletByIdAsync(string palletId);
		IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly? minBestBefore);
		IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter);			
		Task ClearPalletFromListIssueAsync(string palletId);
		Task ChangePalletStatusAsync(string palletId, PalletStatus palletStatus);//może nie być potrzebne		
		Task<string> GetNextPalletIdAsync();		
		Task<Pallet> GetPalletByLocationAsync(int locationId); //numer lokacji	
	}
}
