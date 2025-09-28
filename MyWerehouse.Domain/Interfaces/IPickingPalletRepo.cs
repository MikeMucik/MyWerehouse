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
		//Task DeleteVirtualPickingAsync(int id);
		Allocation AddAllocation(VirtualPallet pallet, int issueId, int quantity);
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(int productId);
		//Task<List<Allocation>> GetAllocationByEntityAsync(Issue issue, Product product );
		Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end);
		Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId);
		//Task<List<Allocation>> GetAllocationsForIssueAsync(int issueId);
		Task<int> GetVirtualPalletIdFromPalletIdAsync(string palletId);
		//Task<string> GetPalletIdFromPalletPickingIdAsync (int palletPickingId);
		//Task<List<PickingPalletDTO>>
		Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate);
		Task<Allocation> GetAllocationAsync(int allocationId);
		Task<VirtualPallet> GetVirtualPalletByIdAsync(int palletId);
		Task<List<Allocation>> GetAllocationsByIssueIdProductIdAsync(int issueId, int productId);
		Task<List<Allocation>> GetAllocationsProductIdAsync(int productId, DateTime from, DateTime to);
		//Task<List<(int IssueId, int TotalQuantity)>> GetNumberIssueAsync(int productId, DateTime? dateTime);
		//Task<List<PickingPallet>> GetPickingPalletsByProductAsync(int productId);
		//TODO
		//Trzeba jeszcze zamykać palety jak są pełne !!
	}
}
