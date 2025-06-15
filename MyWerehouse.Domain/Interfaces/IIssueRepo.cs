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
		Task AddIssueAsync(Issue issue);
		void DeleteIssue(int id);
		Task DeleteIssueAsync(int id);
		void UpdateIssue(Issue issue);
		Task UpdateIssueAsync(Issue issue);		
		Issue? GetIssueById(int id);
		Task<Issue?> GetIssueByIdAsync(int id);
		IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter);
		//IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly minBestBeforeDate);
		//List<Pallet> SelectPalletsForIssue(IQueryable<Pallet> pallet, int quantity);
		//Task<List<Pallet>> SelectPalletsForIssueAsync(IQueryable<Pallet> pallet, int quantity);
	}
}
