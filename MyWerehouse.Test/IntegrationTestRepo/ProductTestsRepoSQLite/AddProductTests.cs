using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Products.ProductsExceptions;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ProductTestsRepoSQLite
{
	public class AddProductTests : TestBase
	{
		[Fact]
		public void AddProperDataWithDetails_AddProduct_ShouldAddToCollection()
		{
			//Arrange			
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var product = Product.Create("Apple", "666666", TestDates.UtcNow, 1, 56, 100, 220, 120, 400, "500");
			var productRepo = new ProductRepo(DbContext);
			//Act
			var result = productRepo.AddProduct(product);
			DbContext.SaveChanges();
			//Assert	
			Assert.NotNull(result);
			var fullResult = DbContext.Products.FirstOrDefault(p => p.Name == product.Name);
			Assert.NotNull(fullResult);
			Assert.NotNull(fullResult.Details);
			Assert.Equal("666666", fullResult.SKU);
			Assert.Equal(100, fullResult.Details.Length);
		}

		[Fact]
		public void AddProduct_ShouldNotAddToCollection_WhenInvalidLength()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			//Act&Assert
			var ex = Assert.Throws<WrongLengthProductDomainException>(() => Product.Create("Banana", "1234567890", TestDates.UtcNow, 1, 56, 0, 220, 120, 400, "500"));
			Assert.Contains("Not correct size of length", ex.Message);
		}
	}
}
