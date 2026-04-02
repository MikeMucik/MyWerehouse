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
			{	Id =1,
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			var product = Product.Create("Banana", "1234567890", 1, 56);
			//var product = new Product
			//{				
			//	Name = "Banana",
			//	SKU = "1234567890",
			//	Category = newCategory,
			//	CartonsPerPallet = 56,
			//};
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
		//Przejeło domain
		//[Fact]
		//public void SwitchOffProduct_SwitchOffProduct_ShouldHideProduct()
		//{
		//	//Arrange
		//	var newCategory = new Category
		//	{
		//		Name = "CategoryName"
		//	};
		//	DbContext.Categories.Add(newCategory);
		//	var product = Product.Create("Banana", "1234567890", 1, 56);
		//	//var product = new Product
		//	//{
		//	//	Name = "Banana",
		//	//	SKU = "1234567890",
		//	//	Category = newCategory,
		//	//	CartonsPerPallet = 56,
		//	//};
		//	DbContext.Products.Add(product);
		//	DbContext.SaveChanges();
		//	var productRepo = new ProductRepo(DbContext);
		//	//Act
		//	productRepo.SwitchOffProduct(product);
		//	DbContext.SaveChanges();
		//	//Assert
		//	var productDeleted = DbContext.Products.Find(product.Id);
		//	Assert.NotNull(productDeleted);
		//	Assert.True(productDeleted.IsDeleted);
		//}
	}
}
