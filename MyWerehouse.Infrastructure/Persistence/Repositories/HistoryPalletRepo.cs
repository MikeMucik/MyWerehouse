using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Histories.Filters;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

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
		public IQueryable<HistoryPallet> GetDataByFilter(HistoryPalletSearchFilter filter, Guid id)
		{
			var query = _werehouseDbContext.HistoryPallet
				.Where(p => p.PalletId == id);

			if (filter.SourceLocationId.HasValue)
				query = query.Where(p => p.SourceLocationId == filter.SourceLocationId);

			if (filter.DestinationLocationId.HasValue)
				query = query.Where(p => p.DestinationLocationId == filter.DestinationLocationId);

			if (filter.Reason != null)
				query = query.Where(p => p.Reason == filter.Reason);

			if (!string.IsNullOrWhiteSpace(filter.PerformedBy))
				query = query.Where(p => p.PerformedBy == filter.PerformedBy);

			if (filter.MovementDateStart.HasValue)
			{
				var start = filter.MovementDateStart.Value;
				var end = filter.MovementDateEnd ?? DateTime.UtcNow;

				query = query.Where(p => p.MovementDate >= start && p.MovementDate <= end);
			}

			if (filter.ProductId.HasValue || filter.Quantity != null)
			{
				query = query.Where(p => p.HistoryPalletDetails.Any(md =>
					(!filter.ProductId.HasValue || md.ProductId == filter.ProductId) &&
					(filter.Quantity == null || md.Quantity == filter.Quantity)
				));
			}
			return query;
		}
		
		public async Task<bool> CanDeletePalletAsync(Guid id)
		{
			int movementCount = await _werehouseDbContext.HistoryPallet
				.Where(p => p.PalletId == id)
				.Take(2)
				.CountAsync();
			return movementCount <= 1;
		}
	}
}