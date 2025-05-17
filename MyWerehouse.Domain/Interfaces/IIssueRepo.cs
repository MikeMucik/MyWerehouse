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
		void UpdateIssue(Issue issue);
		void DeleteIssue(int id);
		Issue GetIssueById(int id);
		IQueryable<Issue> GetIssuesByFilter(IssueReceiptSearchFilter filter);
	}
}
