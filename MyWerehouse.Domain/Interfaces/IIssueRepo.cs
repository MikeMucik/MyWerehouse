using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IIssueRepo
	{
		void AddIssue(Issue issue);
		void DeleteIssue(Issue issue);		
		Task<Issue?> GetIssueByIdAsync(int id);		
		Task<List<Issue>> GetIssuesByIdsAsync(List<int> ids);
		Task<Issue?> GetIssueByIdWithPalletAndItemsAsync(int id, CancellationToken cancellationToken);
		IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter);
		Task<List<PalletWithLocation>> GetPalletByIssueIdAsync(int id);		
	}
}
