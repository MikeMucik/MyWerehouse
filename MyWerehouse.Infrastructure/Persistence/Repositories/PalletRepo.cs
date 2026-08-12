using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class PalletRepo : IPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public Guid AddPallet(Pallet pallet)
		{
			_werehouseDbContext.Pallets.Add(pallet);
			return pallet.Id;
		}

		public async Task<Pallet?> GetPalletByIdAsync(Guid palletId, CancellationToken ct)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.Location)
				.Include(p => p.Receipt)
				.Include(p => p.Issue)
				.FirstOrDefaultAsync(p => p.Id == palletId, ct);
		}

		public async Task<Pallet?> GetPalletByIdFullInfoAsync(Guid palletId, CancellationToken ct)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
				.Include(p => p.PalletHistory)
				.Include(p => p.Location)
				.Include(p => p.Receipt)
				.Include(p => p.Issue)
				.FirstOrDefaultAsync(p => p.Id == palletId, ct);
		}

		public async Task<Pallet?> GetPalletByPalletNumberAsync(string palletNumber, CancellationToken ct)
		{
			return await _werehouseDbContext.Pallets
				.FirstOrDefaultAsync(p => p.PalletNumber == palletNumber, ct);
		}
		public IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(a => a.ProductsOnPallet)
				.Where(p => p.Status != PalletStatus.Archived);

			if (!string.IsNullOrEmpty(filter.PalletNumber))
			{
				result = result.Where(p => p.PalletNumber == filter.PalletNumber);
			}
			if (filter.ProductId.HasValue)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp => pp.ProductId == filter.ProductId));
			}
			if (!string.IsNullOrWhiteSpace(filter.ProductName))
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product != null &&
				EF.Functions.Like(pp.Product.Name.ToLower(), $"%{filter.ProductName.ToLower()}%")));
			}
			if (!string.IsNullOrWhiteSpace(filter.SKU))
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product.SKU == filter.SKU));
			}
			if (filter.LocationBay > 0)
			{
				result = result.Where(p => p.Location.Bay == filter.LocationBay);
			}
			if (filter.LocationAisle > 0)
			{
				result = result.Where(p => p.Location.Aisle == filter.LocationAisle);
			}
			if (filter.LocationPosition > 0)
			{
				result = result.Where(p => p.Location.Position == filter.LocationPosition);
			}
			if (filter.LocationHeight > 0)
			{
				result = result.Where(p => p.Location.Height == filter.LocationHeight);
			}
			if (filter.PalletStatus.HasValue)
			{
				result = result.Where(p => p.Status == filter.PalletStatus);
			}
			if (filter.BestBeforeFrom != null || filter.BestBeforeTo != null)
			{
				var bestBeforeStart = filter.BestBeforeFrom ?? DateOnly.MinValue;
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
				result = result.Where(p => p.Issue != null && p.Issue.PerformedBy == filter.IssueUser);
			}
			return result;
		}

		public async Task<int> ReservePalletNumbersAsync(int count, CancellationToken ct)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

			while (true)
			{
				var counter = await _werehouseDbContext.PalletNumberCounters
					.AsNoTracking()
					.SingleAsync((x => x.Name == "Pallet"), ct);

				var firstNumber = counter.NextNumber;
				var nextFreeNumber = firstNumber + count;

				//mechanizm zapobiegający dubla i ustawiający nową wartość
				var affectedRows = await _werehouseDbContext.PalletNumberCounters
						.Where(x =>	x.Name == "Pallet" && x.NextNumber == firstNumber)
						.ExecuteUpdateAsync(setters =>
						setters.SetProperty(x => x.NextNumber,nextFreeNumber), ct);

				if (affectedRows == 1)
					return firstNumber;
			}
		}

		public async Task<Pallet?> CheckOccupancyAsync(int locationId, CancellationToken ct)
		{
			var pallet = await _werehouseDbContext.Pallets.FirstOrDefaultAsync(p => p.LocationId == locationId, ct);
			return pallet;
		}

		public async Task<Pallet?> GetPickingPalletByIssueId(Guid issueId, CancellationToken ct)
		{
			return await _werehouseDbContext.Pallets
					.Include(p => p.ProductsOnPallet)
					.Where(p => p.IssueId == issueId && p.Status == PalletStatus.Picking)
					.FirstOrDefaultAsync(ct);
		}

		public async Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId, CancellationToken ct)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(m => m.PalletHistory)
				.Where(p => p.ReceiptId == reciptId)
				.ToListAsync(ct);
		}

		public async Task<List<Pallet>> GetAvailablePalletsExcluding(Guid productId, DateOnly? bestBefore, HashSet<Guid> excludedId, CancellationToken ct)
		{
			var pallets = await _werehouseDbContext.Pallets
				.Include(l => l.Location)
				.Include(p => p.ProductsOnPallet)
				.Where(p => !excludedId.Contains(p.Id))
				.Where(p =>
					(p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) &&
					p.ProductsOnPallet.Any(pp =>
						pp.ProductId == productId &&
					(bestBefore == null || pp.BestBefore >= bestBefore)
					)
				)
				.OrderBy(p => p.ProductsOnPallet
					.Where(pp => pp.ProductId == productId)
					.Min(pp => pp.BestBefore ?? DateOnly.MaxValue))

				.ThenBy(p => p.ProductsOnPallet
					.Where(pp => pp.ProductId == productId)
					.Min(pp => pp.Quantity))
				.ThenBy(p => p.LocationId)
				.ThenBy(p => p.DateReceived)
				.Take(10)//nie bierz wszystkich
				.ToListAsync(ct);
			return pallets;
		}

		public async Task<List<Pallet>> GetMissingFullPallets(Guid productId, int fullPallet, DateOnly? minBestBefore, int neededPallets, CancellationToken ct)
		{
			var pallets = await _werehouseDbContext.Pallets
				.Where(p => (p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) &&
					p.ProductsOnPallet.Any(pp => pp.ProductId == productId &&
					(minBestBefore == null || pp.BestBefore >= minBestBefore) && pp.Quantity == fullPallet
				))
				.Include(p => p.Location)
				.OrderBy(p => p.ProductsOnPallet
					.Where(x => x.ProductId == productId)
					.Select(x => x.BestBefore)
					.FirstOrDefault() ?? DateOnly.MaxValue)
				.ThenBy(x => x.Location)
				.Take(neededPallets)
				.Include(p => p.ProductsOnPallet)
				.ToListAsync(ct);
			return pallets;
		}
		public async Task<List<Pallet>> GetAvailablePalletsForReversePickingAsync(Guid productId, DateOnly? bestBefore, Guid sourceId, int cartonsPerPallet, CancellationToken ct)
		{
			var pallets = await _werehouseDbContext.Pallets
				.Include(p => p.Location)
				.Include(p => p.ProductsOnPallet)
				.Where(a => a.ProductsOnPallet.Count == 1 && a.ProductsOnPallet.Any(pp => pp.ProductId == productId && pp.BestBefore == bestBefore)
				&& a.Receipt != null && a.Status == PalletStatus.Available && a.Id != sourceId &&
				a.ProductsOnPallet.Single().Quantity < cartonsPerPallet)
				.OrderByDescending(p => p.ProductsOnPallet.Single().Quantity)
				.ToListAsync(ct);
			return pallets;
		}
	}
}
