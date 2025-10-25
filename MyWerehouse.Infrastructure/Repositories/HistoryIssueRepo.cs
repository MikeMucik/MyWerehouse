using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

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

		public IQueryable<HistoryIssue> GetAllHistoryIssues()
		{
			return _werehouseDbContext.HistoryIssues
				.Include(d=>d.Details)
				.AsQueryable();
		}
	}
}
