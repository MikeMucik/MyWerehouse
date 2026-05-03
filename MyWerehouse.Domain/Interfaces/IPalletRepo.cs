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
		Task<Pallet?> GetPalletByIdAsync(Guid palletId);
		Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId);
		Task<List<Pallet>> GetAvailableFullPallets(Guid productId,int fullPallet, DateOnly? minBestBefore, int neededPallets);
		Task<List<Pallet>> GetAvailablePalletsExcluding(Guid productId, DateOnly? bestBefore, HashSet<Guid> excludedId);
		Task<Pallet?> GetPickingPalletByIssueId(Guid issueId);			
		IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter);				
		Task<string> GetNextPalletIdAsync();		
		Task<Pallet> CheckOccupancyAsync(int locationId); //numer lokacji																
	}
}
