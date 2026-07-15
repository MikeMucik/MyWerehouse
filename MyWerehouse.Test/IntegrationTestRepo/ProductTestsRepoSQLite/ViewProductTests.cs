using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;
using Xunit;

namespace MyWerehouse.Test.IntegrationTestRepo.ProductTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewProductTests
	{
		private readonly ProductRepo _productRepo;
		private readonly QueryTestSQLFixture _fixture;
		public ViewProductTests(QueryTestSQLFixture fixture)		
		{
			_fixture = fixture;
			_productRepo = new ProductRepo(_fixture.DbContext);
		}

		[Fact]
		public void ShowAllProduct_GetAllProducts_ReturnList()
		{
			//Arrange&Act			
			var result = _productRepo.GetAllProducts();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count()); 
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
			Assert.Equal(2, result.Count());
			var resultList = result.ToList();
			foreach (var item in resultList)
			{
				Assert.Contains("Test", item.Name);
			}
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
			Assert.Single(result);
			var resultList = result.ToList();
			Assert.Equal("0987654321", resultList[0].SKU);
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
			Assert.Single(result);
			var resultList = result.ToList();
			var resultSingle = resultList[0];
			Assert.NotNull(resultSingle.Details);
			Assert.Equal(2, resultSingle.Details.Weight);
			Assert.Equal(30, resultSingle.Details.Width);
		}
	}
}
