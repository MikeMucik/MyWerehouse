using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.InventoryTestsRepo
{
	public class DecreaseInventoryQuantityTests :CommandTestBase
	{
		private readonly InventoryRepo _inventoryRepo;
		public DecreaseInventoryQuantityTests() : base()
		{
			_inventoryRepo = new InventoryRepo(_context);
		}
		[Fact]
		public void DeductQuantity_DecreaseInventoryQuantity_DeductQuantityForProduct()
		{
			//Arrange
			var productId = 10;
			var quantity = 5;
			//Act
			_inventoryRepo.DecreaseInventoryQuantity(productId, quantity);
			//Assert
			var result = _context.Inventory.FirstOrDefault(i => i.ProductId == productId);
			Assert.NotNull(result);
			Assert.Equal(quantity, result.Quantity);//DbContextFactory Q=10 -> 10 - 5 =5
			Assert.InRange(result.LastUpdated, DateTime.UtcNow.AddSeconds(-15), DateTime.UtcNow);
		}
		[Fact]
		public void DeductQuantityWhenQuantityToLow_DecreaseInventoryQuantity_ThrowException()
		{
			//Arrange
			var productId = 10;
			var quantity = 15;
			//Act&Assert
			var ex = Assert.Throws<InvalidOperationException>(() =>  _inventoryRepo.DecreaseInventoryQuantity(productId, quantity));
			Assert.Equal("Za mało towaru w magazynie.", ex.Message);			
		}
		[Fact]
		public void DeductQuantityWhenProductIdNotExist_DecreaseInventoryQuantity_ThrowException()
		{
			//Arrange
			var productId = 1000;
			var quantity = 15;
			//Act&Assert
			var ex = Assert.Throws<InvalidOperationException>(() => _inventoryRepo.DecreaseInventoryQuantity(productId, quantity));
			Assert.Equal("Nie można odjąć ze stanu – produkt nie istnieje.", ex.Message);
		}
	}
}
