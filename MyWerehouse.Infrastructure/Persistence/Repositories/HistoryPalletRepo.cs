using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;

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

		public async Task<bool> CanDeletePalletAsync(Guid id, CancellationToken ct)
		{
			int movementCount = await _werehouseDbContext.HistoryPallet
				.Where(p => p.PalletId == id)
				.Take(2)
				.CountAsync(ct);
			return movementCount <= 1;
		}
	}
}
