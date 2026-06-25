using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ProductTestsIntegration
{
	[Collection("QueryCollectionInMemory")]
	public class ViewProductIntegrationTests : CommandTestBase
	{
		private readonly ProductService _productService;
		private readonly ProductRepo _productRepo;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly ICategoryRepo _categoryRepo;

		public ViewProductIntegrationTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_productRepo = new ProductRepo(_context);
			_inventoryRepo = new InventoryRepo(_context);
			_categoryRepo = new CategoryRepo(_context);
			_productService = new ProductService(_productRepo, _mapper, _context, _inventoryRepo, _categoryRepo);
		}
		[Fact]
		public async Task ShowProductDetails_DetailsOfProductAsync_ReturnData()
		{
			//Arrange
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			//Act
			var result = await _productService.DetailsOfProductAsync(productId1);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal("Test", result.Result.Name);
			Assert.Equal("TestDetails", result.Result.Description);
		}
		[Fact]
		public async Task ShowProductDetailsBadId_DetailsOfProductAsync_ThrowException()
		{
			//Arrange
			var productId9 = Guid.Parse("00000000-0000-0000-0009-000000000000");
			//Act
			var result = await _productService.DetailsOfProductAsync(productId9);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			//Assert
		}
		[Fact]
		public async Task ShowProduct_GetProductToEditAsync_ReturnAddProductDTO()
		{
			//Arrange
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			//Act
			var result = await _productService.GetProductToEditAsync(productId1);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.IsType<AppResult<EditProductDTO>>(result);
		}
		[Fact]
		public async Task ShowProducts_GetProductsAsync_ReturnList()
		{
			//Arrange
			var pageSize = 2;
			var pageNumber = 1;
			var ct = CancellationToken.None;
			//Act
			var result = await _productService.GetProductsAsync(pageSize, pageNumber, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(2, result.Result.Items.Count);
			Assert.Equal("Test", result.Result.Items[0].Name);
		}
		[Fact]
		public async Task ShowProducts_FindProductsByFilterAsync_ReturnList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var ct = CancellationToken.None;
			var filter = new ProductSearchFilter
			{
				ProductName = "Test",
			};
			//Act
			var result = await _productService.FindProductsByFilterAsync(pageSize, pageNumber, filter, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(2, result.Result.Items.Count);
			Assert.Equal("Test", result.Result.Items[0].Name);
		}
		[Fact]
		public async Task ShowProducts_FindProductsByEmptyFilterAsync_ReturnList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var ct = CancellationToken.None;
			var filter = new ProductSearchFilter
			{
				
			};
			//Act
			var result = await _productService.FindProductsByFilterAsync(pageSize, pageNumber, filter, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(3, result.Result.Items.Count);
			Assert.Equal("Test", result.Result.Items[0].Name);
		}
		[Fact]
		public async Task ShowNoProducts_FindProductsByFilterAsync_ReturnEmptyList()
		{
			//Arrange
			var pageSize = 3;
			var pageNumber = 1;
			var ct = CancellationToken.None;
			var filter = new ProductSearchFilter
			{
				Length = 1000,
			};
			//Act
			var result = await _productService.FindProductsByFilterAsync(pageSize, pageNumber, filter,ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Empty(result.Result.Items);
		}
	}
}
