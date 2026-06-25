using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ProductTestsRepoSQLite
{
	public class DeleteProductTests : TestBase
	{

		[Fact]
		public void RemoveProduct_DeleteProduct_ShouldRemoveFromCollection()
		{
			//Arrange
			var newCategory = new Category
			{
				Id = 1,
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			var product = Product.Create("Banana", "1234567890", 1, 56);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var productRepo = new ProductRepo(DbContext);
			//Act
			productRepo.DeleteProduct(product);
			DbContext.SaveChanges();
			//Assert
			var productDeleted = DbContext.Products.Find(product.Id);
			Assert.Null(productDeleted);
		}		
	}
}
