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
			_werehouseDbContext.PalletMovement.Add(palletMovement);
			//_werehouseDbContext.SaveChanges();
		}
		public async Task AddPalletMovementAsync(PalletMovement palletMovement)
		{
			await _werehouseDbContext.PalletMovement.AddAsync(palletMovement);
			//await _werehouseDbContext.SaveChangesAsync();
		}
		public IQueryable<PalletMovement> GetDataByFilter(PalletMovementSearchFilter filter)
		{
			var result = _werehouseDbContext.PalletMovement
				.Include(md => md.PalletMovementDetails)
					.ThenInclude(m => m.Product)
				.AsQueryable();
			if (filter.PalletId != null)
			{
				result = result.Where(p => p.PalletId == filter.PalletId);
			}
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
			if (filter.LocationId > 0)
			{
				result = result.Where(p => p.LocationId == filter.LocationId);
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
		public bool CanDeletePallet(string id)
		{
			int movementCount = _werehouseDbContext.PalletMovement
				.Where(p => p.PalletId == id)
				.Take(2)
				.Count();
			return movementCount <= 1;
		}
		public async Task<bool> CanDeletePalletAsync(string id)
		{
			int movementCount = await _werehouseDbContext.PalletMovement
				.Where(p => p.PalletId == id)
				.Take(2)
				.CountAsync();
			return movementCount <= 1;
		}
	}
}
