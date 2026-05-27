using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ProductTestsIntegration
{
	public class AddProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public async Task AddNewProductAsync_ShouldAddToCollection_WhenProperData()
		{
			//Arrange
			var category = new Category
			{
				Name = "qwe"
			};
			_context.Categories.Add(category);
			_context.SaveChanges();
			var productNew = new EditProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
				CartonsPerPallet =56
			};
			//Act			
			var result = await _productService.AddProductAsync(productNew);
			//Assert
			var product = _context.Products.Find(result.Result);
			Assert.NotNull(product);
			Assert.Equal(productNew.Name, product.Name);
			Assert.Equal(productNew.SKU, product.SKU);
			Assert.Equal(productNew.CategoryId, product.CategoryId);

			Assert.Equal(productNew.Description, product.Details.Description);			
			Assert.Equal(productNew.Length, product.Details.Length);
			Assert.Equal(productNew.Height, product.Details.Height);
			Assert.Equal(productNew.Weight, product.Details.Weight);
			Assert.Equal(productNew.Width, product.Details.Width);
		}
		[Fact]
		public async Task AddNewProductAsync_ShouldThrowValidateException_WhenInvalidDataHeight()
		{
			//Arrange
			var productNew = new EditProductDTO
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
		public async Task AddNewProductAsync_ShouldReturnAppResultError_WhenDataNameExist()
		{
			//Arrange
			var product = Product.Create( "Test", "666666", 1, 56);
			
			_context.Products.Add(product);
			_context.SaveChanges();
			var productNew = new EditProductDTO
			{
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
				CartonsPerPallet =56
			};
			//Act
			var result = await _productService.AddProductAsync(productNew);
			//Assert
			Assert.Contains("Produkt o tej nazwie już istnieje.", result.Error);
			Assert.Equal(ErrorType.NotFound, result.ErrorType);
		}
		[Fact]
		public async Task NewProductInvalidDataSKU_AddNewProductAsync_NoAddedToCollection()
		{
			//Arrange
			var productNew = new EditProductDTO
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