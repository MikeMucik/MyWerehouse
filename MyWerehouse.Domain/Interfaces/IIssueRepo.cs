using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IIssueRepo
	{
		void AddIssue(Issue issue);
		void DeleteIssue(Issue issue);	
		Task<Issue?> GetIssueByIdAsync(Guid id);		
		Task<List<Issue>> GetIssuesByIdsAsync(List<Guid> ids);
		Task<Issue?> GetIssueByIdWithPalletAndItemsAsync(Guid id);
		IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter);
		Task<List<PalletWithLocation>> GetPalletByIssueIdAsync(Guid id);
		Task<int> GetNextNumberOfIssue();
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid id);
	}
}
