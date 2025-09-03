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
	public class PalletRepo : IPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task<string> AddPalletAsync(Pallet pallet)
		{
			await _werehouseDbContext.Pallets.AddAsync(pallet);
			return pallet.Id;
		}
		public async Task DeletePalletAsync(string palletId)
		{
			var pallet = await _werehouseDbContext.Pallets.FindAsync(palletId);
			if (pallet != null)
			{
				_werehouseDbContext.Pallets.Remove(pallet);
			}
		}
		public async Task<Pallet?> GetPalletByIdAsync(string palletId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.Include(p => p.Receipt)
				.Include(p => p.Issue)
				.FirstOrDefaultAsync(p => p.Id == palletId);
		}
		public IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Where(p => p.Status != PalletStatus.Archived);
			if (filter.ProductId > 0)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp => pp.ProductId == filter.ProductId));
			}
			if (!string.IsNullOrWhiteSpace(filter.ProductName))
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product != null &&
				EF.Functions.Like(pp.Product.Name.ToLower(), $"%{filter.ProductName.ToLower()}%")));
			}
			if (filter.LocationId > 0)
			{
				result = result.Where(p => p.LocationId == filter.LocationId);
			}
			if (filter.PalletStatus.HasValue)
			{
				result = result.Where(p => p.Status == filter.PalletStatus);
			}
			if (filter.BestBefore != null || filter.BestBeforeTo != null)
			{
				var bestBeforeStart = filter.BestBefore ?? DateOnly.MinValue;
				var bestBeforeEnd = filter.BestBeforeTo ?? DateOnly.MaxValue;
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.BestBefore >= bestBeforeStart && pp.BestBefore <= bestBeforeEnd));
			}
			if (filter.StartDate != null)
			{
				var start = filter.StartDate.Value;
				var end = filter.EndDate ?? DateTime.Now;

				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.DateAdded >= start && pp.DateAdded <= end));
			}
			if (filter.ClientIdIn != null)
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.ClientId == filter.ClientIdIn);
			}
			if (filter.ClientIdOut != null)
			{
				result = result.Where(p => p.Issue != null && p.Issue.ClientId == filter.ClientIdOut);
			}
			if (!string.IsNullOrEmpty(filter.ReceiptUser))
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.PerformedBy == filter.ReceiptUser);
			}
			if (!string.IsNullOrEmpty(filter.IssueUser))
			{
				result = result.Where(p => p.Issue != null && p.Issue.PerformedBy == filter.IssueUser);//dodaj test
			}
			return result;
		}
		public IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly? minBestBefore)
		{
			var pallets = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p =>
					(p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) &&
					p.ProductsOnPallet.Any(pp =>
						pp.ProductId == productId &&
						pp.BestBefore >= minBestBefore
					)
				)
				.OrderBy(p => p.ProductsOnPallet
					.Where(pp => pp.ProductId == productId)
					.Min(pp => pp.BestBefore))
				.ThenBy(p => p.LocationId);
			return pallets;
		}
		public async Task ClearPalletFromListIssueAsync(string palletId)
		{
			var pallet = await _werehouseDbContext.Pallets
				.FirstOrDefaultAsync(p => p.Id == palletId);
			pallet.IssueId = null;
			pallet.Status = PalletStatus.Available;
		}
		public async Task ChangePalletStatusAsync(string palletId, PalletStatus palletStatus)
		{
			var pallet =await _werehouseDbContext.Pallets
				.FirstOrDefaultAsync(p => p.Id == palletId);
			switch (palletStatus)
			{
				case PalletStatus.ToIssue:
					pallet.Status = PalletStatus.ToIssue;
					break;
				case PalletStatus.Damaged:
					pallet.Status = PalletStatus.Damaged;
					break;
				case PalletStatus.OnHold:
					pallet.Status = PalletStatus.OnHold;
					break;
				case PalletStatus.Loaded:
					pallet.Status = PalletStatus.Loaded;
					break;
				case PalletStatus.ToPicking:
					pallet.Status = PalletStatus.ToPicking;
					break;
				case PalletStatus.Archived:
					pallet.Status = PalletStatus.Archived;
					break;
				default:
					pallet.Status = PalletStatus.Available;
					break;
			}
		}
		public async Task<string> GetNextPalletIdAsync()
		{
			var lastPallet = await _werehouseDbContext.Pallets
				.Where(p => p.Id.StartsWith("Q"))
				.OrderByDescending(p => p.Id)
				.FirstOrDefaultAsync();

			int lastNumber = 0;
			if (lastPallet != null && int.TryParse(lastPallet.Id.AsSpan(1), out var parsed))
			{
				lastNumber = parsed;
			}
			string nextId = $"Q{(lastNumber + 1).ToString("D4")}";
			return nextId;
		}
		public async Task<Pallet> GetPalletByLocationAsync(int locationId)
		{
			var pallet = await _werehouseDbContext.Pallets.FirstOrDefaultAsync(p => p.LocationId == locationId);
			return pallet;
		}
	}
}
