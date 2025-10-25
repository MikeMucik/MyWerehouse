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
	public class PalletMovementRepo : IPalletMovementRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PalletMovementRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddPalletMovement(PalletMovement palletMovement)
		{
			_werehouseDbContext.PalletMovements.Add(palletMovement);			
		}
		public IQueryable<PalletMovement> GetDataByFilter(PalletMovementSearchFilter filter, string id)
		{
			var result = _werehouseDbContext.PalletMovements				
				.Include(md => md.PalletMovementDetails)
					.ThenInclude(m => m.Product)
				.Where(p=>p.PalletId == id)
				.AsQueryable();

			if (filter.ProductId > 0)
			{
				result = result
					.Where(p => p.PalletMovementDetails.Any(md => md.ProductId == filter.ProductId));
			}
			if (!string.IsNullOrWhiteSpace(filter.ProductName))
			{
				result = result
					.Where(p => p.PalletMovementDetails
					.Any(md => md.Product.Name != null && md.Product.Name
					.Contains(filter.ProductName, StringComparison.CurrentCultureIgnoreCase)));
			}
			if (filter.SourceLocationId > 0)
			{
				result = result.Where(p => p.SourceLocationId == filter.SourceLocationId);
			}
			if (filter.DestinationLocationId > 0)
			{
				result = result.Where(p => p.DestinationLocationId == filter.DestinationLocationId);
			}
			if (filter.Reason != null)
			{
				result = result.Where(p => p.Reason == filter.Reason);
			}
			if (filter.PerformedBy != null)
			{
				result = result.Where(p => p.PerformedBy == filter.PerformedBy);
			}
			if (filter.Quantity != null)
			{
				result = result
					.Where(p => p.PalletMovementDetails.Any(md => md.Quantity == filter.Quantity));
			}
			if (filter.MovementDateStart != null)
			{
				var start = filter.MovementDateStart.Value;
				var end = filter.MovementDateEnd ?? DateTime.Now;

				result = result.Where(p =>
				p.MovementDate >= start && p.MovementDate <= end);
			}
			return result;
		}		
		public async Task<bool> CanDeletePalletAsync(string id)
		{
			int movementCount = await _werehouseDbContext.PalletMovements
				.Where(p => p.PalletId == id)
				.Take(2)
				.CountAsync();
			return movementCount <= 1;
		}		
	}
}
