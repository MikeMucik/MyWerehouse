using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewProductTests
	{
		private readonly ProductRepo _productRepo;
		public ViewProductTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_productRepo = new ProductRepo(_context);
		}		
		
		[Fact]
		public void ShowAllProduct_GetAllProducts_ReturnList()
		{
			//Arrange
			//Act
			var result = _productRepo.GetAllProducts();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count()); //3->Factory
		}
		[Fact]
		public void ByName_FindProduct_ShowProductContainsWord()
		{
			//Arrange
			var filter = new ProductSearchFilter { ProductName = "Test" };
			//Act
			var result = _productRepo.FindProducts(filter);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<Product>>(result);
			Assert.Equal(2, result.Count()); //bo dwa zaczynają się na test
		}
		[Fact]
		public void BySKUAndWidth_FindProduct_ShowProductsWithSKU()
		{
			//Arrange			
			var filter = new ProductSearchFilter { SKU = "0987654321" };
			//Act
			var result = _productRepo.FindProducts(filter);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<Product>>(result);
			Assert.Equal(1, result.Count());
		}
		[Fact]
		public void ByWeightAndWidth_FindProduct_ShowProductsWithWeightAndWidth()
		{
			//Arrange
			var filter = new ProductSearchFilter { Weight = 2, Width = 30 };			
			//Act
			var result = _productRepo.FindProducts(filter);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<Product>>(result);
			Assert.Equal(1, result.Count());
		}
	}
}
