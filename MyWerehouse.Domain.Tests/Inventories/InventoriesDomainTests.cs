using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Inventories.InventoryExceptions;
using MyWerehouse.Domain.Inventories.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Inventories
{
	public class InventoriesDomainTests
	{
		[Fact]
		public void CreateStockItem_ShouldCreate_WhenQuantityAboveZero()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var data = TestDates.Now.AddDays(-1);
			//Act
			var inventory = Inventory.CreateStockItem(productId, 1, data);
			//Assert
			Assert.NotNull(inventory);
			Assert.Equal(productId, inventory.ProductId);
			Assert.Equal(1, inventory.Quantity);
			Assert.Equal(data, inventory.LastUpdated);
		}
		[Fact]
		public void CreateStockItem_ShouldCreate_WhenQuantityEqualZero()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var data = TestDates.Now.AddDays(-1);
			//Act
			var inventory = Inventory.CreateStockItem(productId, 0, data);
			//Assert
			Assert.NotNull(inventory);
			Assert.Equal(productId, inventory.ProductId);
			Assert.Equal(0, inventory.Quantity);
			Assert.Equal(data, inventory.LastUpdated);
		}
		[Fact]
		public void CreateStockItem_ThrowException_WhenQuantityBelowZero()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var data = TestDates.Now.AddDays(-1);
			//Act
			var ex = Assert.Throws<InventoryQuantityDomainException>(() => Inventory.CreateStockItem(productId, -1, data));
			//Assert
			Assert.NotNull(ex);
			Assert.Equal(ErrorType.InternalError, ex.ErrorType);
		}

		[Fact]
		public void ApplyChangeInventory_ShouldChange_WhenProperData()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var data = TestDates.Now.AddDays(-1);
			var dataApply = TestDates.Now;
			var inventory = Inventory.CreateStockItem(productId, 1, data);

			//Act
			inventory.ApplyChangeInInventory(2, dataApply);

			//Assert
			Assert.Equal(3, inventory.Quantity);
			Assert.Equal(dataApply, inventory.LastUpdated);
		}
		[Fact]
		public void ApplyChangeInventory_ShouldChange_WhenResultWillBeZero()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var data = TestDates.Now.AddDays(-1);
			var dataApply = TestDates.Now;
			var inventory = Inventory.CreateStockItem(productId, 1, data);

			//Act
			inventory.ApplyChangeInInventory(-1, dataApply);

			//Assert
			Assert.Equal(0, inventory.Quantity);
			Assert.Equal(dataApply, inventory.LastUpdated);
		}
		[Fact]
		public void ApplyChangeInventory_ShouldThrowException_WhenAmountWillBeBelowZero()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var data = TestDates.Now.AddDays(-1);
			var dataApply = TestDates.Now;
			var inventory = Inventory.CreateStockItem(productId, 2, data);
			//Act&Assert
			var ex = Assert.Throws<InventoryQuantityDomainException>(() => inventory.ApplyChangeInInventory(-3, dataApply));
			Assert.Equal(productId, ex.ProductId);
			Assert.Equal(2, inventory.Quantity);
			Assert.Equal(data, inventory.LastUpdated);
			Assert.Equal(ErrorType.InternalError, ex.ErrorType);
		}
	}
}
