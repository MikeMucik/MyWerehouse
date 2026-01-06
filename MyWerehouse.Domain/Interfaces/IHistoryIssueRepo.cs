using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHistoryIssueRepo
	{
		void AddHistoryIssue (HistoryIssue issue);
		Task AddHistoryIssueAsync (HistoryIssue issue, CancellationToken cancellationToken);
		IQueryable<HistoryIssue> GetAllHistoryIssues();		
	}
}
