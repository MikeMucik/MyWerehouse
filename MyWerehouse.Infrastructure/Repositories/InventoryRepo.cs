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
	public class InventoryRepo : IInventoryRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public InventoryRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void IncreaseInventoryQuantity(int productId, int quantity)//zmiana ilości po przyjęciu
		{
			var inventoryItem = _werehouseDbContext.Inventories
				.SingleOrDefault(i => i.ProductId == productId);

			if (inventoryItem != null)
			{
				inventoryItem.Quantity += quantity;
				inventoryItem.LastUpdated = DateTime.UtcNow;
			}
			else
			{
				_werehouseDbContext.Inventories.Add(new Inventory
				{
					ProductId = productId,
					Quantity = quantity,
					LastUpdated = DateTime.UtcNow
				});
			}
			_werehouseDbContext.SaveChanges();
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
		public void DecreaseInventoryQuantity(int productId, int quantity)//zmiana ilości po wydaniu
		{
			var inventoryItem = _werehouseDbContext.Inventories	
				.SingleOrDefault(i => i.ProductId == productId) ?? throw new InvalidOperationException("Nie można odjąć ze stanu – produkt nie istnieje.");
			if (inventoryItem.Quantity < quantity)
				throw new InvalidOperationException("Za mało towaru w magazynie.");

			inventoryItem.Quantity -= quantity;
			inventoryItem.LastUpdated = DateTime.UtcNow;

			_werehouseDbContext.SaveChanges();
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
		public Inventory? GetInventoryForProduct(int productId)//pobranie danych/ilość dla produktu z ostatniej aktualizacji
		{
			var result = _werehouseDbContext.Inventories
				.Include(i => i.Product)				
				.FirstOrDefault(p => p.ProductId == productId);
			return result;
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
		public void UpdateInventory(int productId, int quantity) //zmiana ilości po ręcznej inwenturze
		{
			var result = _werehouseDbContext.Inventories
				.FirstOrDefault(p => p.ProductId == productId);
			if (result == null)
			{
				throw new InvalidOperationException("Nie znaleziono towaru do zaktualizowania");
			}
			result.Quantity = quantity;
			_werehouseDbContext.SaveChanges();
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
		public bool HasStock(int productId, int quantity)
		{
			var quantityBased = _werehouseDbContext.Inventories
				.FirstOrDefault(p => p.ProductId == productId);
			if (quantityBased == null) return false;
			return quantityBased.Quantity >= quantity;
		}
		public async Task<bool> HasStockAsync(int productId, int quantity)
		{
			var quantityBased = await _werehouseDbContext.Inventories
				.FirstOrDefaultAsync(p => p.ProductId == productId);
			if (quantityBased == null) return false;
			return quantityBased.Quantity >= quantity;
		}
	}
}
