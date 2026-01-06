using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Infrastructure.Repositories
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

		public async Task AddHistoryIssueAsync(HistoryIssue issue, CancellationToken cancellationToken)
		{
			await _werehouseDbContext.HistoryIssues.AddAsync(issue, cancellationToken);
		}

		public IQueryable<HistoryIssue> GetAllHistoryIssues()
		{
			return _werehouseDbContext.HistoryIssues
				.Include(d=>d.Details)
				.AsQueryable();
		}
	}
}
