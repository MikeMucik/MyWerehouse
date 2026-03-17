using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public async Task AddHistoryReversePickingAsync(HistoryReversePicking historyReversePicking, CancellationToken cancellationToken)
		{
			await _werehouseDbContext.AddAsync(historyReversePicking, cancellationToken);
		}
	}
}
