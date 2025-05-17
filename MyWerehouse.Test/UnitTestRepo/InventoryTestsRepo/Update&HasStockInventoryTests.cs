using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public void ChangeQuantityInRecord_UpdateInventory_UpdateQuantity()
		{
			//Arrange
			var productId = 10;
			var quantity = 8;
			//Act
			_inventoryRepo.UpdateInventory(productId, quantity);
			//Assert
			var result = _context.Inventory.FirstOrDefault(p=>p.ProductId == productId);	
			Assert.NotNull(result);
			Assert.Equal(quantity, result.Quantity);
		}
		[Fact]
		public void ChangeQuantityInNotExistingRecord_UpdateInventory_ThrowException()
		{
			//Arrange
			var productId = 190;
			var quantity = 8;
			//Act&Assert
			var ex = Assert.Throws<InvalidOperationException>(() => _inventoryRepo.UpdateInventory(productId, quantity));
			Assert.Equal("Nie znaleziono towaru do zaktualizowania", ex.Message);
		}
		[Fact]
		public void CheckStockEnough_HasStock_ReturnTrue()
		{
			//Arrange
			var productId = 10;
			var quantity = 8;
			//Act
			var result = _inventoryRepo.HasStock(productId, quantity);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public void CheckStockNotEnough_HasStock_ReturnFalse()
		{
			//Arrange
			var productId = 10;
			var quantity = 80;
			//Act
			var result = _inventoryRepo.HasStock(productId, quantity);
			//Assert
			Assert.False(result);
		}
	}
}
