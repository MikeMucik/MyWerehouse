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
		public void ProperId_GetProductById_ReturnAllDates()
		{
			//Arrange
			var id = 10;
			//Act
			var result = _productRepo.GetProductById(id);
			//Assert
																																																																					
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal(10, result.Details.Length); // 10 -> FactoryBase
		}
		[Fact]
		public void NotProperId_GetProductById_ReturnAllDates()
		{
			//Arrange
			var id = -1;
			//Act
			var result = _productRepo.GetProductById(id);
			//Assert
			Assert.Equal(null, result);
		}
		[Fact]
		public void ShowAllProduct_GetAllProducts_ReturnList()
		{
			//Arrange
			//Act
			var result = _productRepo.GetAllProducts();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count()); //2->Factory
		}
		[Fact]
		public void ByName_FindProduct_ShowOneProduct()
		{
			//Arrange
			//string name = "TestD";
			//Act
			var result = _productRepo.FindProduct("","",0,0,0,2);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<Product>>(result);
			Assert.Equal(1, result.Count());
		}
	}
}
