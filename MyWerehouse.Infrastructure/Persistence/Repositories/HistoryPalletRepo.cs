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
	public class HistoryPalletRepo : IHistoryPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public HistoryPalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddHistoryPallet(HistoryPallet historyPallet)
		{
			_werehouseDbContext.HistoryPallet.Add(historyPallet);			
		}		
		
		public async Task<bool> CanDeletePalletAsync(Guid id)
		{
			int movementCount = await _werehouseDbContext.HistoryPallet
				.Where(p => p.PalletId == id)
				.Take(2)
				.CountAsync();
			return movementCount <= 1;
		}

		public async Task<List<HistoryPallet>> GetHistoryPallet(string PalletNumber)
		{
			var query = await _werehouseDbContext.HistoryPallet
				.Include(hd=>hd.HistoryPalletDetails)
				.Where(p=>p.PalletNumber == PalletNumber)
				.ToListAsync();
			return query;
		}
	}
}