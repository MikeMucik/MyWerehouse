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
	public class AddProductTests : TestBase
	{		
		[Fact]
		public void AddProperData_AddProduct_ShouldAddToCollection()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var product = Product.Create("Banana", "1234567890", 1,56);			
			var productRepo = new ProductRepo(DbContext);
			//Act
			var result = productRepo.AddProduct(product);
			DbContext.SaveChanges();
			//Assert	
			Assert.NotNull(result);
			var fullResult = DbContext.Products.FirstOrDefault(p => p.Name == product.Name);
			Assert.NotNull(fullResult);
			Assert.Equal("1234567890", fullResult.SKU);
		}
		
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
			var product = Product.Create("Apple", "666666", 1, 56);
			var productDetail = ProductDetail.CreateDetails(product.Id, 100, 220, 120, 400, "500");
			product.SetDetails(productDetail);			
			var productRepo = new ProductRepo(DbContext);
			//Act
			var result = productRepo.AddProduct(product);
			DbContext.SaveChanges();
			//Assert	
			Assert.NotNull(result); 
			var fullResult = DbContext.Products.FirstOrDefault(p => p.Name == product.Name);
			Assert.NotNull(fullResult);
			Assert.Equal("666666", fullResult.SKU);
			Assert.Equal(100, fullResult.Details.Length);		
		}
	}
}
