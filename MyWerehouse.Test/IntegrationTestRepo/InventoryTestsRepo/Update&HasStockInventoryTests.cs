using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.InventoryTestsRepo
{
	public class Update_HasStockTests : CommandTestBase
	{
		private readonly InventoryRepo _inventoryRepo;
		public Update_HasStockTests() : base()
		{
			_inventoryRepo = new InventoryRepo(_context);
		}		
		[Fact]
		public async Task ChangeQuantityInRecord_UpdateInventoryAsync_UpdateQuantity()
		{
			//Arrange
			var initialQuantity = 10;
			var initialProductId = 10;
			var initialInventory = new Inventory { ProductId = initialProductId, Quantity = initialQuantity, LastUpdated = DateTime.UtcNow.AddDays(-1) };
			_context.Inventories.Add(initialInventory);
			_context.SaveChanges();
			//Act
			var productId = 10;
			var quantity = 8;
			await _inventoryRepo.UpdateInventoryAsync(productId, quantity);
			//Assert
			var result = _context.Inventories.FirstOrDefault(p => p.ProductId == productId);
			Assert.NotNull(result);
			Assert.Equal(quantity, result.Quantity);
		}		
		[Fact]
		public async Task ChangeQuantityInNotExistingRecord_UpdateInventoryAsync_ThrowException()
		{
			//Arrange
			var initialQuantity = 10;
			var initialProductId = 10;
			var initialInventory = new Inventory { ProductId = initialProductId, Quantity = initialQuantity, LastUpdated = DateTime.UtcNow.AddDays(-1) };
			_context.Inventories.Add(initialInventory);
			_context.SaveChanges();
			//Act&Assert
			var productId = 190;
			var quantity = 8;			
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _inventoryRepo.UpdateInventoryAsync(productId, quantity));
			Assert.Equal("Nie znaleziono towaru do zaktualizowania", ex.Message);
		}		
		[Fact]
		public async Task CheckStockEnough_HasStockAsync_ReturnTrue()
		{
			//Arrange
			var initialQuantity = 10;
			var initialProductId = 10;
			var initialInventory = new Inventory { ProductId = initialProductId, Quantity = initialQuantity, LastUpdated = DateTime.UtcNow.AddDays(-1) };
			_context.Inventories.Add(initialInventory);
			_context.SaveChanges();
			//Act
			var productId = 10;
			var quantity = 8;			
			var result = await _inventoryRepo.HasStockAsync(productId, quantity);
			//Assert
			Assert.True(result);
		}		
		[Fact]
		public async Task CheckStockNotEnough_HasStockAsync_ReturnFalse()
		{
			//Arrange
			var initialQuantity = 10;
			var initialProductId = 10;
			var initialInventory = new Inventory { ProductId = initialProductId, Quantity = initialQuantity, LastUpdated = DateTime.UtcNow.AddDays(-1) };
			_context.Inventories.Add(initialInventory);
			_context.SaveChanges();
			//Act
			var productId = 10;
			var quantity = 80;			
			var result = await _inventoryRepo.HasStockAsync(productId, quantity);
			//Assert
			Assert.False(result);
		}
	}
}
