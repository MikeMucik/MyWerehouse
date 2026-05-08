using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class HistoryReversePickingRepo : IHistoryReversePickingRepo
	{
		private WerehouseDbContext _werehouseDbContext;
		public HistoryReversePickingRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void AddHistoryReversePicking(HistoryReversePicking historyReversePicking)
		{
			_werehouseDbContext.HistoryReversePickings.Add(historyReversePicking);
		}

		public async Task<List<HistoryReversePicking>> GetHistoryReversePickings()
		{
			return await _werehouseDbContext.HistoryReversePickings.ToListAsync();
		}
	}
}
