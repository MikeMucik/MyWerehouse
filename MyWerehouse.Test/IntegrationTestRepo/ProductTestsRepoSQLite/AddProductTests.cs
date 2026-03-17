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
			var product = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			var productRepo = new ProductRepo(DbContext);
			//Act
			var result = productRepo.AddProduct(product);
			DbContext.SaveChanges();
			//Assert			
			var fullResult = DbContext.Products.FirstOrDefault(p => p.Name == product.Name);
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
			var product = new Product
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Details = new ProductDetail
				{
					Length = 100,
					Height = 200,
					Width = 300,
					Weight = 400,
					Description = "500",
				},
				CartonsPerPallet = 56,
			};
			var productRepo = new ProductRepo(DbContext);
			//Act
			var result = productRepo.AddProduct(product);
			DbContext.SaveChanges();
			//Assert			
			var fullResult = DbContext.Products.FirstOrDefault(p => p.Name == product.Name);
			Assert.Equal("666666", fullResult.SKU);
			Assert.Equal(100, fullResult.Details.Length);		
		}
	}
}
