using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Pallets.Models;

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
				.ThenInclude(p => p.Product)
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
		
		public async Task<int> ReservePalletNumbersAsync(int count, CancellationToken ct)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

			while (true)
			{
				var counter = await _werehouseDbContext.NumberCounters
					.AsNoTracking()
					.SingleAsync((x => x.Name == "Pallet"), ct);

				var firstNumber = counter.NextNumber;
				var nextFreeNumber = firstNumber + count;

				//mechanizm zapobiegający dubla i ustawiający nową wartość
				var affectedRows = await _werehouseDbContext.NumberCounters
						.Where(x => x.Name == "Pallet" && x.NextNumber == firstNumber)
						.ExecuteUpdateAsync(setters =>
						setters.SetProperty(x => x.NextNumber, nextFreeNumber), ct);

				if (affectedRows == 1)
					return firstNumber;
			}
		}

		public async Task<Pallet?> CheckOccupancyAsync(int locationId, CancellationToken ct)
		{
			var pallet = await _werehouseDbContext.Pallets
				.FirstOrDefaultAsync(p => p.LocationId == locationId, ct);
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
		public async Task<List<PalletAllocationCandidate>> GetCandidates(Guid productId, DateOnly? bestBefore, HashSet<Guid> excludedId, CancellationToken ct)
		{
			var candidates = await _werehouseDbContext.Pallets
				.Where(p => !excludedId.Contains(p.Id))
				.Where(p =>
					(p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) && p.ProductsOnPallet.Count == 1 &&
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
				.ThenBy(p=>p.Id)
				.Select(p=> new PalletAllocationCandidate(
					p.Id,
					p.ProductsOnPallet
					.Where(x=>x.ProductId == productId)
					.Sum(x=>x.Quantity)))
				.ToListAsync(ct);
			return candidates;
		}
		public async Task<List<Pallet>> GetSelectedPallets(List<Guid> guids, CancellationToken ct)
		{
			var pallets = await _werehouseDbContext.Pallets
				.Include(l=>l.Location)
				.Include(pp=>pp.ProductsOnPallet)
				.Where(p=> guids.Contains(p.Id))
				.ToListAsync(ct);
			var order = new Dictionary<Guid, int>();

			for (var index = 0; index < guids.Count; index++)
			{
				var palletId = guids[index];
				order[palletId] = index;
			}

			return pallets
				.OrderBy(pallet => order[pallet.Id])
				.ToList();
		}
		public async Task<List<Pallet>> GetMissingFullPallets(Guid productId, int fullPallet, DateOnly? minBestBefore, int neededPallets, CancellationToken ct)
		{
			var pallets = await _werehouseDbContext.Pallets
				.Where(p => (p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) && p.ProductsOnPallet.Count ==1 &&
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
