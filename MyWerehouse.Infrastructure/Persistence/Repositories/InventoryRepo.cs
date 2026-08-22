using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Inventories.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class InventoryRepo : IInventoryRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public InventoryRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddInventory(Inventory inventory)
		{
			_werehouseDbContext.Inventories.Add(inventory);
		}
		public async Task<Inventory?> GetInventoryForProductAsync(Guid productId, CancellationToken ct)//pobranie danych/ilość dla produktu z ostatniej aktualizacji
		{
			var result = await _werehouseDbContext.Inventories
				.Include(i => i.Product)
				.SingleOrDefaultAsync(p => p.ProductId == productId, ct);
			return result;
		}
		public IQueryable<Inventory> GetAllInventory()
		{
			return _werehouseDbContext.Inventories;
		}
		public async Task<bool> HasStockAsync(Guid productId, int quantity, CancellationToken ct)
		{
			var quantityBased = await _werehouseDbContext.Inventories
				.FirstOrDefaultAsync(p => p.ProductId == productId, ct);
			if (quantityBased == null) return false;
			return quantityBased.Quantity >= quantity;
		}
		public async Task<int> GetAllocatableQuantityAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct)
		{
			// 1. pełne dostępne palety
			var fullPalletsQuery = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p => (p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock)&& p.ProductsOnPallet.Count ==1)
				.AsQueryable();

			if (bestBefore.HasValue)
			{
				fullPalletsQuery = fullPalletsQuery
					.Where(p => p.ProductsOnPallet.Any(pop =>
						pop.ProductId == productId && pop.BestBefore >= bestBefore));
			}

			var totalFromFullPallets = await fullPalletsQuery
				.SelectMany(p => p.ProductsOnPallet)
				.Where(pop => pop.ProductId == productId)
				.SumAsync(pop => pop.Quantity, ct);

			// 2. palety rozbite (ToPicking)
			var pickingQuery = _werehouseDbContext.VirtualPallets
				.Include(pp => pp.Pallet)
				.Where(pp => pp.Pallet.Status == PalletStatus.ToPicking && pp.Pallet.ProductsOnPallet.Count == 1&&
							 pp.Pallet.ProductsOnPallet.Any(pop => pop.ProductId == productId));

			if (bestBefore.HasValue)
			{
				pickingQuery = pickingQuery.Where(pp => pp.Pallet.ProductsOnPallet
					.Any(pop => pop.ProductId == productId && pop.BestBefore >= bestBefore));
			}

			var totalFromPicking = await pickingQuery
				.Select(pp => pp.InitialPalletQuantity - (pp.PickingTasks.Sum(a => (int?)a.RequestedQuantity) ?? 0))
				.SumAsync(ct);

			return totalFromFullPallets + totalFromPicking;
		}
		public async Task<List<Inventory>> GetInventoriesForProductsAsync(List<Guid> productIds, CancellationToken ct)
		{
			return await _werehouseDbContext.Inventories
				.Where(i => productIds.Contains(i.ProductId))
				.ToListAsync(ct);
		}
	}
}
