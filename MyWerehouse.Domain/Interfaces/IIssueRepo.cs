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
		Task AddIssueAsync(Issue issue);		
		Task DeleteIssueAsync(int id);		
		Task UpdateIssueAsync(Issue issue);			
		Task<Issue?> GetIssueByIdAsync(int id);
		Task<Issue?> GetIssueForLoadAsync(int id);
		Task <List<Issue>> GetIssuesByIdsAsync(List<int> ids);
		IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter);
		Task <List<PalletWithLocation>> GetPalletByIssueIdAsync(int id);			
	}
}
