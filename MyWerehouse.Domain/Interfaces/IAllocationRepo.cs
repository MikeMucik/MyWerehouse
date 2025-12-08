using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IAllocationRepo
	{
		void AddAllocation(Allocation allocation);
		void DeleteAllocation(Allocation allocation);
		Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate);
		Task<Allocation> GetAllocationAsync(int allocationId);
		Task<List<Allocation>> GetAllocationsByIssueIdProductIdAsync(int issueId, int productId);
		Task<List<Allocation>> GetAllocationsByIssueIdAsync(int issueId);
		Task<List<Allocation>> GetAllocationsProductIdAsync(int productId, DateTime from, DateTime to);
		Task<List<VirtualPallet>> GetVirtualPalletsByIssue(int issueId);
		//Task AddAllocationApartFromCreatingIssueAsync(int issueId, int productId, int quantity);
		//dodanie dodatkowej alokacji gdy jest problem stanfizyczny/system
		//tworzone przez biuro tzw. "BRAKI"
		//Task ChangeStatusPalletToPicked(int allocationId);
		//Task <List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate);
		//Task <Allocation> GetAllocationAsync(int allocationId);
	}
}
