using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.InventoryTestsRepoSQLite
{
	public class AddInventoryTests : TestBase
	{
		[Fact]
		public void AddproperData_AddInventory_ShouldAddToCollection()
		{
			//Arrange
			var category = new Category
			{
				Name = "TestC",
			};
			var product = new Product
			{
				Category = category,
				Name = "TestP",
				SKU = "1234Test",
			};
			var quantity = 10;
			var date = DateTime.UtcNow;
			var inventory = new Inventory
			{
				Product = product,
				Quantity = quantity,
				LastUpdated = date,
			};
			var inventoryRepo = new InventoryRepo(DbContext);
			//Act
			inventoryRepo.AddInventory(inventory);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Inventories.FirstOrDefault(a=>a.ProductId == inventory.ProductId);
			Assert.NotNull(result);	
		}		
	}
}
