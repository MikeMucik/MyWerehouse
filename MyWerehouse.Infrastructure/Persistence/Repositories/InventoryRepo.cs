using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Models;
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
		public async Task<Inventory?> GetInventoryForProductAsync(int productId)//pobranie danych/ilość dla produktu z ostatniej aktualizacji
		{
			var result = await _werehouseDbContext.Inventories
				.Include(i => i.Product)
				.SingleOrDefaultAsync(p => p.ProductId == productId);
			return result;
		}
		//Na razie zbędne
		public IQueryable<Inventory> GetAllInventory()
		{
			return _werehouseDbContext.Inventories;
		}				
		public async Task<bool> HasStockAsync(int productId, int quantity)
		{
			var quantityBased = await _werehouseDbContext.Inventories
				.FirstOrDefaultAsync(p => p.ProductId == productId);
			if (quantityBased == null) return false;
			return quantityBased.Quantity >= quantity;
		}
		//Na razie zbędne - chyba do wywalenia
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
				.Select(pp => pp.InitialPalletQuantity - (pp.PickingTasks.Sum(a =>(int?) a.RequestedQuantity) ?? 0))
				.SumAsync();

			return totalFromFullPallets + totalFromPicking;
		}

		public async Task<int> GetQuantityForProductAsync(int productId, DateOnly? bestBefore)
		{
			var query = GetPalletsQuery(productId, bestBefore)
				.Where(p => p.Status != PalletStatus.OnHold &&
				p.Status != PalletStatus.Archived &&
				p.Status != PalletStatus.Damaged);			
			return await SumQuantityAsync(query, productId);
		}
		public async Task<int> GetQuantityProductReservedForIssueAsync(int productId, DateOnly? bestBefore)
		{
			var query = GetPalletsQuery(productId, bestBefore)
				.Where(p => p.Status == PalletStatus.ToIssue );
			//|| p.IssueId != 0
			return await SumQuantityAsync(query, productId);
		}
		public async Task<int> GetQuantityProductReservedForPickingAsync(int productId, DateOnly? bestBefore)
		{
			var palletsWithProduct = _werehouseDbContext.VirtualPallets
				.Include(p=>p.Pallet)

				.Where(p=>p.Pallet.Status == PalletStatus.ToPicking &&
				p.Pallet.ProductsOnPallet.Any(pop=>pop.ProductId == productId))		
				
				.AsQueryable();
			if (bestBefore.HasValue)
			{
				palletsWithProduct = palletsWithProduct
					.Where(pp => pp.Pallet.ProductsOnPallet
					.Any(pop => pop.ProductId == productId && pop.BestBefore >= bestBefore));
			}
			var totalAllocated = await palletsWithProduct
			   .SelectMany(pp => pp.PickingTasks)
			   .SumAsync(a => (int?)a.RequestedQuantity) ?? 0;
			return totalAllocated;
		}
		private IQueryable<Pallet> GetPalletsQuery(int productId, DateOnly? bestBefore)
		{
			 var palletsWithProduct = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				 .Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == productId))
				.AsQueryable();
			if (bestBefore.HasValue)
			{
				palletsWithProduct = palletsWithProduct
					.Where(p => p.ProductsOnPallet.Any(pop =>
						 pop.BestBefore >= bestBefore));
			}
			return palletsWithProduct;
		}
		private static async Task<int> SumQuantityAsync(IQueryable<Pallet> pallets, int productId)
		{
			var totalFromPallets = await pallets
				.SelectMany(p => p.ProductsOnPallet)
				.Where(pop => pop.ProductId == productId)
				.SumAsync(pop => pop.Quantity);
			return totalFromPallets;
		}

		//public async Task AddInventoryAsync(Inventory inventory)
		//{
		//	await _werehouseDbContext.Inventories.AddAsync(inventory);
		//}

		public async Task<List<Inventory>> GetInventoriesForProductsAsync(List<int> productIds)
		{
			return await _werehouseDbContext.Inventories
				.Where(i=> productIds.Contains(i.ProductId))
				.ToListAsync();
		}
	}
}