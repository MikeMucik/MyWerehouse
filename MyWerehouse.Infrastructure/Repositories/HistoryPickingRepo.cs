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
	public class HistoryPickingRepo : IHistoryPickingRepo
	{
		private WerehouseDbContext _werehouseDbContext;
		public HistoryPickingRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task AddHistoryPickingAsync(HistoryPicking historyPicking)
		{
			await _werehouseDbContext.HistoryPickings.AddAsync(historyPicking);
		}

		public IQueryable<HistoryPicking> GetAllHistoryPickingAsync()
		{
			return _werehouseDbContext.HistoryPickings
				.AsQueryable();
				
		}
	}
}
