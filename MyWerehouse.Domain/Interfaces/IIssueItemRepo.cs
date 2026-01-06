using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IIssueItemRepo
	{
		void AddIssueItem(IssueItem issueItem);
		Task<IssueItem> GetIssueItemAsync(int id);
		Task<int> GetQuantityByIssueAndProduct(Issue issue, int productId);
		void DeleteIssueItem(IssueItem issue);
	}
}
