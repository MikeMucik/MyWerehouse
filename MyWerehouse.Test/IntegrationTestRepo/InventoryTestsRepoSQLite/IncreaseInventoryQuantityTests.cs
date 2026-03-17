using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.InventoryTestsRepo
{
	public class IncreaseInventoryQuantityTests :CommandTestBase
	{
		private readonly InventoryRepo _inventoryRepo;
		public IncreaseInventoryQuantityTests(): base()
		{
			_inventoryRepo = new InventoryRepo(_context);
		}		
		//[Fact]
		//public async Task AddNewProductAndQuanatity_IncreaseInventoryQuantityAsync_AddNewRecord()
		//{
		//	//Arrange
		//	var productId = 99;
		//	var quantity = 10;
		//	//Act
		//	await _inventoryRepo.IncreaseInventoryQuantityAsync(productId, quantity);
		//	_context.SaveChanges();
		//	//Assert
		//	var result = _context.Inventories.FirstOrDefault(i => i.ProductId == productId);
		//	Assert.NotNull(result);
		//	Assert.Equal(quantity, result.Quantity);
		//	Assert.InRange(result.LastUpdated, DateTime.UtcNow.AddSeconds(-15), DateTime.UtcNow);
		//}
		//[Fact]
		//public async Task AddQuanatityToExistRecord_IncreaseInventoryQuantityAsync_UpdateNewRecord()
		//{
		//	//Arrange
		//	var initialQuantity = 10;
		//	var initialProductId = 10;
		//	var initialInventory = new Inventory { ProductId = initialProductId, Quantity = initialQuantity, LastUpdated = DateTime.UtcNow.AddDays(-1) };
		//	_context.Inventories.Add(initialInventory);
		//	_context.SaveChanges();
		//	//Act
		//	var productId = 10;
		//	var quantity = 10;
		//	var before = DateTime.UtcNow;
		//	await _inventoryRepo.IncreaseInventoryQuantityAsync(productId, quantity);
		//	var after = DateTime.UtcNow;
		//	//Assert
		//	var result = _context.Inventories.FirstOrDefault(i => i.ProductId == productId);
		//	Assert.NotNull(result);
		//	Assert.Equal(quantity + 10, result.Quantity);//+10 DbContextFactory													   
		//	Assert.InRange(result.LastUpdated, before, after);
		//}
	}
}
