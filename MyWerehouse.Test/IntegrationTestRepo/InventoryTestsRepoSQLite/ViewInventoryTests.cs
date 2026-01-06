using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.InventoryTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewInventoryTests: TestBase
	{
		public readonly InventoryRepo _inventoryRepo;
		private readonly QueryTestFixture _fixture;
		public ViewInventoryTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_inventoryRepo = new InventoryRepo(_fixture.DbContext);
		}		
		[Fact]
		public async Task ShowDataByProductId_GetInventoryForProductAsync_ReturnInfoOfQuantity()
		{
			//Arrange
			var productId = 10;
			//Act
			var result =await _inventoryRepo.GetInventoryForProductAsync(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(productId, result.ProductId);
			Assert.Equal(10, result.Quantity); //DbContextFactory Q=10			
		}
		[Fact]
		public void ShowAllData_GetAllInventory_ReturnListOfInventory()
		{
			//Arrange&Act
			var result = _inventoryRepo.GetAllInventory();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			Assert.Equal(10, result.FirstOrDefault(p=>p.ProductId == 10).Quantity);			
		}
		[Fact]
		public async Task CheckStockEnough_HasStockAsync_ReturnTrue()
		{
			//Arrange&Act			
			var productId = 10;
			var quantity = 8;
			var result = await _inventoryRepo.HasStockAsync(productId, quantity);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public async Task CheckStockNotEnough_HasStockAsync_ReturnFalse()
		{
			//Arrange&Act
			var productId = 10;
			var quantity = 80;
			var result = await _inventoryRepo.HasStockAsync(productId, quantity);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task ReturnAmount_GetQuantityProductReservedForPickingAsync_GiveProperQuantity()
		{
			//Arrange
			var productId = 11;
			var bestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365));			
			//Act
			var result =await _inventoryRepo.GetQuantityProductReservedForPickingAsync(productId, bestBefore);
			//Assert
			Assert.Equal(90, result);
		}
		[Fact]
		public async Task ReturnAmount_GetQuantityProductReservedForIssueAsync_GiveProperQuantity()
		{
			//Arrange
			var productId = 11;
			var bestBefore = new DateOnly(2025, 12, 12);
			//var
			//Act
			var result = await _inventoryRepo.GetQuantityProductReservedForIssueAsync(productId, bestBefore);
			//Assert
			Assert.Equal(200, result);
		}
		[Fact]
		public async Task ReturnAmount_GetAvailableQuantityAsync_GiveBackQuantity()
		{
			//Arrange
			var productId = 11;
			var bestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
			//var
			//Act
			var result = await _inventoryRepo.GetAvailableQuantityAsync(productId, bestBefore);
			//Assert
			Assert.Equal(660, result);
		}
		[Fact]
		public async Task ReturnAmount_GetQuantityForProductAsync_GiveBackQuantity()
		{
			//Arrange
			var productId = 11;
			var bestBefore = new DateOnly(2025, 12, 12);
			//var
			//Act
			var result = await _inventoryRepo.GetQuantityForProductAsync(productId, bestBefore);
			//Assert
			Assert.Equal(970, result);
		}
	}
}
