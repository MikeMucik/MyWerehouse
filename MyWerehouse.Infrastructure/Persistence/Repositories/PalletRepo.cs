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

		public async Task<Pallet?> GetPalletByIdAsync(Guid palletId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				//.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.Include(p => p.Receipt)
				.Include(p => p.Issue)
				.FirstOrDefaultAsync(p => p.Id == palletId);
		}
		public IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(a => a.ProductsOnPallet)
				.Where(p => p.Status != PalletStatus.Archived);

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
		//może powinno być pobierz dostępne palety ale tylko tyle ile potrzebuje, zwracaj ProductOnPallet wraz z Pallet
		//public IQueryable<Pallet> GetAvailablePallets(Guid productId, DateOnly? minBestBefore)
		//{
		//	var pallets = _werehouseDbContext.Pallets
		//		.Include(p => p.ProductsOnPallet)
		//		.Where(p =>
		//			(p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) &&
		//			p.ProductsOnPallet.Any(pp =>
		//				pp.ProductId == productId &&
		//				pp.BestBefore >= minBestBefore
		//			)
		//		)
		//		.OrderBy(p => p.ProductsOnPallet
		//			.Where(pp => pp.ProductId == productId)
		//			.Min(pp => pp.BestBefore))
		//		.ThenBy(p => p.LocationId)
		//		.ThenBy(p => p.DateReceived)
		//		.Take(10);
		//	//nie bierz wszystkich

		//	return pallets;
		//}

		public async Task<string> GetNextPalletIdAsync()
		{
			var lastPallet = await _werehouseDbContext.Pallets
				.Where(static p => p.PalletNumber.StartsWith("Q"))
				.OrderByDescending(p => p.PalletNumber)
				.FirstOrDefaultAsync();

			//var lastPallet = await _werehouseDbContext.Pallets.MaxAsync(p => p.PalletNumber);

			int lastNumber = 0;
			if (lastPallet != null && int.TryParse(lastPallet.PalletNumber.AsSpan(1), out var parsed))
			//if (lastPallet != null && int.TryParse(lastPallet.AsSpan(1), out var parsed))
			{
				lastNumber = parsed;
			}
			string nextId = $"Q{(lastNumber + 1).ToString("D4")}";
			return nextId;
		}
		public async Task<Pallet> CheckOccupancyAsync(int locationId)
		{
			var pallet = await _werehouseDbContext.Pallets.FirstOrDefaultAsync(p => p.LocationId == locationId);
			return pallet;
		}

		public async Task<Pallet?> GetPickingPalletByIssueId(Guid issueId)
		{
			return await _werehouseDbContext.Pallets
					.Include(p => p.ProductsOnPallet)
					.Where(p => p.IssueId == issueId && p.Status == PalletStatus.Picking)
					.FirstOrDefaultAsync();
		}

		public async Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(m => m.PalletMovements)
				.Where(p => p.ReceiptId == reciptId)
				.ToListAsync();
		}

		public async Task<List<Pallet>> GetAvailablePalletsExcluding(Guid productId, DateOnly? bestBefore, HashSet<Guid> excludedId)
		{
			var pallets = await _werehouseDbContext.Pallets
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
					.Min(pp => pp.BestBefore ??DateOnly.MaxValue))
				
				.ThenBy(p => p.ProductsOnPallet//ThenBy
					.Where(pp => pp.ProductId == productId)
					.Min(pp => pp.Quantity))
				.ThenBy(p => p.LocationId)
				.ThenBy(p => p.DateReceived)
				.Take(10)//nie bierz wszystkich
				.ToListAsync();
			return pallets;
		}

		public async Task<List<Pallet>> GetAvailableFullPallets(Guid productId, int fullPallet, DateOnly? minBestBefore, int neededPallets)
		{
			var pallets = await _werehouseDbContext.Pallets
				.Where(p => (p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) &&
					p.ProductsOnPallet.Any(pp => pp.ProductId == productId &&
					(minBestBefore == null || pp.BestBefore >= minBestBefore) && pp.Quantity == fullPallet
				))
				.OrderBy(p => p.ProductsOnPallet
					.Where(x => x.ProductId == productId)
					.Select(x => x.BestBefore)
					.FirstOrDefault() ?? DateOnly.MaxValue)
				.ThenBy(x => x.Location)
				.Take(neededPallets)
				.Include(p => p.ProductsOnPallet)
				.ToListAsync();
			return pallets;
		}

		//public void DeletePallet(Pallet pallet)
		//{			
		//		_werehouseDbContext.Pallets.Remove(pallet);			
		//}
		//public async Task<bool> ExistsAsync(string palletId, int productId)
		//{

		//	return await _werehouseDbContext.ProductOnPallet
		//		.AnyAsync(p => p.PalletId == palletId && p.ProductId == productId);
		//}
	}
}
//.OrderBy(p => p.ProductsOnPallet
//	.Where(pp => pp.ProductId == productId)
//	.Min(pp => pp.BestBefore))
//.ThenBy(p => p.LocationId)
//.ThenBy(p => p.DateReceived)
//.Take(neededPallets)
//.ToList();//nie bierz wszystkich

//.Select(p => new
//{
//	Pallet = p,
//	Qty = p.ProductsOnPallet
//		.Where(x => x.ProductId == productId)
//		.Select(x => x.Quantity)
//		.FirstOrDefault(),
//	BestBefore = p.ProductsOnPallet
//		.Where(x => x.ProductId == productId)
//		.Select(x => x.BestBefore)
//		.FirstOrDefault(),
//});
//var palletList = await pallets
//	.OrderBy(x => x.BestBefore ?? DateOnly.MaxValue)
//	.ThenBy(x => x.Pallet.Location)
//	.ToListAsync();
//var fullPallets = palletList
//	.Where(x => x.Qty == fullPallet)
//	.Take(neededPallets)
//	.Select(x => x.Pallet)
//	.ToList();