using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class HistoryIssueRepo : IHistoryIssueRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public HistoryIssueRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void AddHistoryIssue(HistoryIssue issue)
		{
			_werehouseDbContext.HistoryIssues.Add(issue);
		}

		public IQueryable<HistoryIssue> GetAllHistoryIssues()
		{
			return _werehouseDbContext.HistoryIssues
				.Include(d=>d.Details)
				.AsQueryable();
		}
	}
}
