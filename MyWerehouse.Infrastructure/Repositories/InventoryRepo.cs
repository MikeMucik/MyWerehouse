using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class InventoryRepo : IInventoryRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public InventoryRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task AddInventoryAsync(int productId, int quantity)
		{
			var inventory = new Inventory
			{
				ProductId = productId,
				Quantity = quantity,
				LastUpdated = DateTime.Now,
			};
			await _werehouseDbContext.Inventories.AddAsync(inventory);
		}
		public async Task IncreaseInventoryQuantityAsync(int productId, int quantity)//zmiana ilości po przyjęciu
		{
			var inventoryItem = await _werehouseDbContext.Inventories
				.SingleOrDefaultAsync(i => i.ProductId == productId);

			if (inventoryItem != null)
			{
				inventoryItem.Quantity += quantity;
				inventoryItem.LastUpdated = DateTime.UtcNow;
			}
			else
			{
				await _werehouseDbContext.Inventories.AddAsync(new Inventory
				{
					ProductId = productId,
					Quantity = quantity,
					LastUpdated = DateTime.UtcNow
				});
			}
			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task DecreaseInventoryQuantityAsync(int productId, int quantity)//zmiana ilości po wydaniu
		{
			var inventoryItem = await _werehouseDbContext.Inventories
				.SingleOrDefaultAsync(i => i.ProductId == productId) ?? throw new InvalidOperationException("Nie można odjąć ze stanu – produkt nie istnieje.");
			if (inventoryItem.Quantity < quantity)
				throw new InvalidOperationException("Za mało towaru w magazynie.");

			inventoryItem.Quantity -= quantity;
			inventoryItem.LastUpdated = DateTime.UtcNow;

			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task<Inventory?> GetInventoryForProductAsync(int productId)//pobranie danych/ilość dla produktu z ostatniej aktualizacji
		{
			var result = await _werehouseDbContext.Inventories
				.Include(i => i.Product)
				.FirstOrDefaultAsync(p => p.ProductId == productId);
			return result;
		}
		public IQueryable<Inventory> GetAllInventory()
		{
			return _werehouseDbContext.Inventories;
		}		
		public async Task UpdateInventoryAsync(int productId, int quantity) //zmiana ilości po ręcznej inwenturze
		{
			var result = await _werehouseDbContext.Inventories
				.FirstOrDefaultAsync(p => p.ProductId == productId);
			if (result == null)
			{
				throw new InvalidOperationException("Nie znaleziono towaru do zaktualizowania");
			}
			result.Quantity = quantity;
			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task<bool> HasStockAsync(int productId, int quantity)
		{
			var quantityBased = await _werehouseDbContext.Inventories
				.FirstOrDefaultAsync(p => p.ProductId == productId);
			if (quantityBased == null) return false;
			return quantityBased.Quantity >= quantity;
		}

		public async Task<int> GetAvailableQuantityAsync(int productId, DateOnly? bestBefore)
		{
			// 1. pełne dostępne palety
			var fullPalletsQuery = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p => p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock)
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
				.SumAsync(pop => pop.Quantity);

			// 2. palety rozbite (ToPicking)
			var pickingQuery = _werehouseDbContext.VirtualPallets
				.Include(pp => pp.Pallet)
				.Where(pp => pp.Pallet.Status == PalletStatus.ToPicking &&
							 pp.Pallet.ProductsOnPallet.Any(pop => pop.ProductId == productId));

			if (bestBefore.HasValue)
			{
				pickingQuery = pickingQuery.Where(pp => pp.Pallet.ProductsOnPallet
					.Any(pop => pop.ProductId == productId && pop.BestBefore >= bestBefore));
			}

			//var totalFromPicking = await pickingQuery.SumAsync(pp => pp.RemainingQuantity);
			var totalFromPicking = await pickingQuery
				.Select(pp => pp.IssueInitialQuantity - (pp.Allocation.Sum(a =>(int?) a.Quantity) ?? 0))
				.SumAsync();

			return totalFromFullPallets + totalFromPicking;
		}

	}
}
//public async Task<int> GetAvailableQuantityAsync(int productId, DateOnly? bestBefore)
//{
//	// filtr na BestBefore
//	Expression<Func<ProductOnPallet, bool>> bestBeforeFilter = pop =>
//		pop.ProductId == productId &&
//		(!bestBefore.HasValue || pop.BestBefore >= bestBefore.Value);

//	// 1. pełne dostępne palety
//	var totalFromFullPallets = await _werehouseDbContext.Pallets
//		.Where(p => p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock)
//		.SelectMany(p => p.ProductsOnPallet)
//		.Where(bestBeforeFilter)
//		.SumAsync(pop => pop.Quantity);

//	// 2. palety rozbite (ToPicking)
//	var totalFromPicking = await _werehouseDbContext.PickingPallets
//		.Where(pp => pp.Pallet.Status == PalletStatus.ToPicking &&
//					 pp.Pallet.ProductsOnPallet.Any(bestBeforeFilter))
//		.Select(pp => pp.IssueInitialQuantity - (pp.Allocation.Sum(a => (int?)a.Quantity) ?? 0))
//		.SumAsync();

//	return totalFromFullPallets + totalFromPicking;
//}
