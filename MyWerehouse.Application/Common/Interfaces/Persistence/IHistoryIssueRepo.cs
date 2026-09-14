using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IHistoryIssueRepo
	{
		void AddHistoryIssue (HistoryIssue issue);
		IQueryable<HistoryIssue> GetAllHistoryIssues();		
	}
}
