using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPalletRepo
	{		
		Guid AddPallet(Pallet pallet);		
		//void DeletePallet(Pallet pallet);		
		Task<Pallet?> GetPalletByIdAsync(Guid palletId);
		Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId);
		IQueryable<Pallet> GetAvailablePallets(Guid productId, DateOnly? minBestBefore);
		Task<Pallet?> GetPickingPalletByIssueId(Guid issueId);			
		IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter);			
		void ClearPalletFromListIssue(Pallet pallet);
		void ChangePalletStatus(Guid palletId, PalletStatus palletStatus);//być może przuda się do innych metod		
		Task<string> GetNextPalletIdAsync();		
		Task<Pallet> CheckOccupancyAsync(int locationId); //numer lokacji
														  //			
	}
}
