using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewProductIntegrationTests(QuerryTestFixture fixture) : ProductIntegrationView(fixture)
	{		
		

		[Fact]
		public async Task ShowProductDetails_DetailsOfProductAsync_ReturnData()
		{
			//Arrange
			var productId = 10;
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");

			//Act
			var result = await _productService.DetailsOfProductAsync(productId1);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("Test", result.Result.Name);
			Assert.Equal("TestDetails", result.Result.Description);
		}
		[Fact]
		public async Task ShowProductDetailsBadId_DetailsOfProductAsync_ThrowException()
		{
			//Arrange
			var productId = 90;
			var productId9 = Guid.Parse("00000000-0000-0000-0009-000000000000");

			//Act

			var result = await _productService.DetailsOfProductAsync(productId9);
			//Assert
			//Assert.Null(result);
			Assert.False(result.IsSuccess);
			//Assert
		}
		[Fact]
		public async Task ShowProduct_GetProductToEditAsync_ReturnAddProductDTO()
		{
			//Arrange
			var productId = 10;
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");

			//Act
			var result = await _productService.GetProductToEditAsync(productId1);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<AppResult<AddProductDTO>>(result);
		}
		[Fact]
		public async Task ShowProducts_GetProductsAsync_ReturnList()
		{
			//Arrange
			var pageSize = 2;
			var pageNumber = 1;
			//Act
			var result = await _productService.GetProductsAsync(pageSize, pageNumber);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Products.Count);
			Assert.Equal("Test", result.Result.Products.First().Name);
		}
		[Fact]
		public async Task ShowProducts_FindProductsByFilterAsync_ReturnList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var filter = new ProductSearchFilter
			{
				ProductName = "Test",
			};
			//Act
			var result = await _productService.FindProductsByFilterAsync(pageSize, pageNumber, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Products.Count);
			Assert.Equal("Test", result.Result.Products.First().Name);
		}
		[Fact]
		public async Task ShowNoProducts_FindProductsByFilterAsync_ReturnEmptyList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var filter = new ProductSearchFilter
			{
				Length = 1000,
			};
			//Act
			var result = await _productService.FindProductsByFilterAsync(pageSize, pageNumber, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Empty(result.Result.Products);
		}
	}
}
