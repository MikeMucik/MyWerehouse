using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;

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

		public async Task<List<HistoryReversePicking>> GetHistoryReversePickings(CancellationToken ct)
		{
			return await _werehouseDbContext.HistoryReversePickings.ToListAsync(ct);
		}
	}
}
