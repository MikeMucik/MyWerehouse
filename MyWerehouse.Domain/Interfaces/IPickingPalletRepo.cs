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
		VirtualPallet AddPalletToPicking(VirtualPallet virtualPallet);		
		void DeleteVirtualPalletPicking(VirtualPallet virtualPallet);			
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(int productId);		
		Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end);
		Task<List<VirtualPallet>> GetVirtualPalletsByBBAsync(int productId, DateOnly bestBefore);
		//Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId);
		Task<int> GetVirtualPalletIdFromPalletIdAsync(string palletId);			
		Task<VirtualPallet> GetVirtualPalletByIdAsync(int palletId);			
		void ClosePickingPallet(string palletId, int issueId);
		//Task<List<VirtualPallet>> GetVirtualPalletsByIssue(int  issueId);
		//Allocation AddAllocation(VirtualPallet pallet, Issue issue, int quantity);
		//void DeleteAllocation(Allocation allocation);
		//Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate);
		//Task<Allocation> GetAllocationAsync(int allocationId);
		//Task<List<Allocation>> GetAllocationsByIssueIdProductIdAsync(int issueId, int productId);
		//Task<List<Allocation>> GetAllocationsByIssueIdAsync(int issueId);
		//Task<List<Allocation>> GetAllocationsProductIdAsync(int productId, DateTime from, DateTime to);
	}
}
