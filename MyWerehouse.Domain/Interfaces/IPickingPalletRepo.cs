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
		Task<VirtualPallet> AddPalletToPickingAsync(string palletId);		
		Task DeleteVirtualPickingAsync(int id);
		void DeleteAllocation(Allocation allocation);// czy na pewno synchroniczna
		Allocation AddAllocation(VirtualPallet pallet, Issue issue, int quantity);
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(int productId);		
		Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end);
		Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId);
		Task<int> GetVirtualPalletIdFromPalletIdAsync(string palletId);		
		Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate);
		Task<Allocation> GetAllocationAsync(int allocationId);
		Task<VirtualPallet> GetVirtualPalletByIdAsync(int palletId);
		Task<List<Allocation>> GetAllocationsByIssueIdProductIdAsync(int issueId, int productId);
		Task<List<Allocation>> GetAllocationsByIssueIdAsync(int issueId);
		Task<List<Allocation>> GetAllocationsProductIdAsync(int productId, DateTime from, DateTime to);		
		Task ClosePickingPalletAsync(string palletId, int issueId);
	}
}
