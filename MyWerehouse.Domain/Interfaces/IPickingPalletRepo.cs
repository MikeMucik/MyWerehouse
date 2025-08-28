using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPickingPalletRepo
	{
		Task AddPalletToPickingAsync(string palletId);		
		Task DeletePalletPickingAsync(int id);
		Task AddAllocationAsync(PickingPallet pallet, int issueId, int quantity);
		Task<List<PickingPallet>> GetPickingPalletsAsync(int productId);
		Task<List<PickingPallet>> GetPickingPalletsByTimeAsync(DateTime start, DateTime end);
		Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId);
		//Task<List<Allocation>> GetAllocationsForIssueAsync(int issueId);
		Task<int> GetPickingPalletIdFromPalletIdAsync (string palletId);
		//Task<string> GetPalletIdFromPalletPickingIdAsync (int palletPickingId);
		//Task<List<PickingPalletDTO>>
		Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate);
		Task<Allocation> GetAllocationAsync(int allocationId);
	}
}
