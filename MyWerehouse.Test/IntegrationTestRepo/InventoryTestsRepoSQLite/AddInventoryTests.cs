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
			{ Id =1,
				Name = "TestC",
			};
			var product = Product.Create("TestP", "1234Test", 1, 56);
			//var product = new Product
			//{
			//	Category = category,
			//	Name = "TestP",
			//	SKU = "1234Test",
			//};
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var quantity = 10;
			var date = DateTime.UtcNow;
			var inventory = new Inventory
			{
				//ProductId = product.Id,
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
