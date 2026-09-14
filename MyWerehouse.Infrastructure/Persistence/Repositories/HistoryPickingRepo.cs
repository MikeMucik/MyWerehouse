using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class HistoryPickingRepo : IHistoryPickingRepo
	{
		private WerehouseDbContext _werehouseDbContext;
		public HistoryPickingRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void AddHistoryPicking(HistoryPicking historyPicking)
		{
			_werehouseDbContext.HistoryPickings.Add(historyPicking);
		}

		public async Task AddHistoryPickingAsync(HistoryPicking historyPicking, CancellationToken cancellationToken)
		{
			await _werehouseDbContext.AddAsync(historyPicking, cancellationToken);
		}

		public IQueryable<HistoryPicking> GetAllHistoryPickingAsync()
		{
			return _werehouseDbContext.HistoryPickings
				.AsQueryable();				
		}
	}
}
