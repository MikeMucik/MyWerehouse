using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IIssueItemRepo
	{
		//Task AddIssueItem(int  issueId, int productId, int quantity, DateOnly bestBefore);
		Task AddIssueItemAsync(IssueItem issueItem);
		Task<IssueItem> GetIssueItemAsync(int id);
		Task<int> GetQuantityByIssueAndProduct(Issue issue, int productId);
		void DeleteIssueItem(IssueItem issue);
	}
}
