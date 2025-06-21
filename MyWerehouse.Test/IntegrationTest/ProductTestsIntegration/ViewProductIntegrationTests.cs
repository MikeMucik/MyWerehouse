using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewProductIntegrationTests(QuerryTestFixture fixture) : ProductIntegrationView(fixture)
	{
		[Fact]
		public void ShowProductDetails_DetailsOfProduct_ReturnData()
		{
			//Arrange
			var productId = 10;
			//Act
			var result = _productService.DetailsOfProduct(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("Test", result.Name);
			Assert.Equal("TestDetails", result.Description);
		}
		[Fact]
		public void ShowProductDetailsBadId_DetailsOfProduct_ThrowException()
		{
			//Arrange
			var productId = 90;
			//Act
			var result = _productService.DetailsOfProduct(productId);
			//Assert
			Assert.Null(result);
		}
		[Fact]
		public void ShowProduct_GetProductToEdit_ReturnAddProductDTO()
		{
			//Arrange
			var productId = 10;
			//Act
			var result = _productService.GetProductToEdit(productId);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<AddProductDTO>(result);
		}
		[Fact]
		public void ShowProducts_GetProducts_ReturnList()
		{
			//Arrange
			var pageSize = 2;
			var pageNumber = 1;
			//Act
			var result = _productService.GetProducts(pageSize, pageNumber);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Products.Count);
			Assert.Equal("Test", result.Products.First().Name);
		}
		[Fact]
		public void ShowProducts_FindProductsByFilter_ReturnList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var filter = new ProductSearchFilter
			{
				ProductName = "Test",
			};
			//Act
			var result = _productService.FindProductsByFilter(pageSize, pageNumber, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Products.Count);
			Assert.Equal("Test", result.Products.First().Name);
		}
		[Fact]
		public void ShowNoProducts_FindProductsByFilter_ReturnEmptyList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var filter = new ProductSearchFilter
			{
				Length = 1000,
			};
			//Act
			var result = _productService.FindProductsByFilter(pageSize, pageNumber, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Empty(result.Products);
		}

		[Fact]
		public async Task ShowProductDetails_DetailsOfProductAsync_ReturnData()
		{
			//Arrange
			var productId = 10;
			//Act
			var result = await _productService.DetailsOfProductAsync(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("Test", result.Name);
			Assert.Equal("TestDetails", result.Description);
		}
		[Fact]
		public async Task ShowProductDetailsBadId_DetailsOfProductAsync_ThrowException()
		{
			//Arrange
			var productId = 90;
			//Act
			var result = await _productService.DetailsOfProductAsync(productId);
			//Assert
			Assert.Null(result);
		}
		[Fact]
		public async Task ShowProduct_GetProductToEditAsync_ReturnAddProductDTO()
		{
			//Arrange
			var productId = 10;
			//Act
			var result = await _productService.GetProductToEditAsync(productId);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<AddProductDTO>(result);
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
			Assert.Equal(2, result.Products.Count);
			Assert.Equal("Test", result.Products.First().Name);
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
			Assert.Equal(2, result.Products.Count);
			Assert.Equal("Test", result.Products.First().Name);
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
			Assert.Empty(result.Products);
		}
	}
}
