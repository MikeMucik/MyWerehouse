using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.InventoryTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewInventoryTests:CommandTestBase
	{
		public readonly InventoryRepo _inventoryRepo;
		public ViewInventoryTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_inventoryRepo = new InventoryRepo(_context);
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
		//[Fact]
		//public void ReturnAmount_GetQuantityProductReservedForPickingAsync_GiveProperQuantity()
		//{
		//	//Arrange
		//	var productId = 1;
		//	var bestBefore = new DateOnly(2025, 12, 12);
		//	//var
		//	//Act
		//	var result = _inventoryRepo.GetQuantityProductReservedForPickingAsync(productId, bestBefore);
		//	//Assert
		//	Assert.NotNull(result);
		//}
	}
}
