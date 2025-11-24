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
		void DeletePallet(Pallet pallet);		
		Task<Pallet?> GetPalletByIdAsync(string palletId);
		IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly? minBestBefore);
		IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter);			
		void ClearPalletFromListIssue(Pallet pallet);
		void ChangePalletStatus(string palletId, PalletStatus palletStatus);//być może przuda się do innych metod		
		Task<string> GetNextPalletIdAsync();		
		Task<Pallet> CheckOccupancyAsync(int locationId); //numer lokacji
														  //			
	}
}
