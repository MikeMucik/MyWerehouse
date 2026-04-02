using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	public class AddProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public async Task NewProductProperData_AddNewProductAsync_AddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act			
			var result = await _productService.AddProductAsync(productNew);
			//Assert
			var product = _context.Products.Find(result.Result);
			Assert.NotNull(product);
			Assert.Equal(productNew.Length, product.Details.Length);
		}
		[Fact]
		public async Task NewProductInvalidDataHeight_AddNewProductAsync_NoAddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				//Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act&Assert
			var ex =await Assert.ThrowsAsync<ValidationException>(() => _productService.AddProductAsync(productNew));
			Assert.Contains("Uzupełnij dane - wysokość", ex.Message);
		}
		[Fact]
		public async Task NewProductInvalidDataName_AddNewProductAsync_NoAddedToCollection()
		{
			//Arrange
			var product = Product.Create( "Test", "666666", 1, 56);
			//var product = new Product
			//{

			//	Name = "Test",
			//	SKU = "666666",
			//	CategoryId = 1,
			//	IsDeleted = false,
			//};
			_context.Products.Add(product);
			_context.SaveChanges();
			var productNew = new AddProductDTO
			{
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act&Assert
			//Act
			var result = await _productService.AddProductAsync(productNew);
			//Assert
			Assert.Contains("Produkt o tej nazwie już istnieje.", result.Error);
			Assert.Equal(ErrorType.NotFound, result.ErrorType);
			//var ex =await Assert.ThrowsAsync<NotFoundProductException>(() => _productService.AddProductAsync(productNew));
			//Assert.Contains("Produkt o tej nazwie już istnieje.", ex.Message);
		}
		[Fact]
		public async Task NewProductInvalidDataSKU_AddNewProductAsync_NoAddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				//SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act&Assert
			var ex =await Assert.ThrowsAsync<ValidationException>(() => _productService.AddProductAsync(productNew));
			Assert.Contains("Uzupełnij dane - SKU", ex.Message);
		}
	}
}
