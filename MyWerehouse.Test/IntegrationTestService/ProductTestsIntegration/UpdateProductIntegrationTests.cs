using System;
using System.Collections.Generic;
using FluentValidation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	public class UpdateProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public void ProperData_UpdateProduct_ChangeData()
		{
			// Arrange
			var details = new ProductDetail
			{				
				ProductId = 1,
				Height = 1,
				Width = 1,
				Length = 1,
				Weight = 1,
				Description = "Test"
			};
			var updatingProduct = new Product
			{
				Id = 1,
				Name = "Test",
				CategoryId = 1,
				SKU = "dede",
				IsDeleted = false,
				AddedItemAd = new DateTime(2024, 2, 2),
				Details = details
			};			
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();			
			//Act
			var updatedProduct = new AddProductDTO
			{
				Id = 1,
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted=false,
				AddedItemAd = DateTime.Now,
				//DetailsId = 1,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",

			};
				_productService.UpdateProduct(updatedProduct);				
			//Assert
				var result = _context.Products
					.Include(d=>d.Details)
					.FirstOrDefault(x => x.Id == updatingProduct.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedProduct.Name, result.Name);
				Assert.Equal(updatedProduct.SKU, result.SKU);
				Assert.Equal(updatedProduct.CategoryId, result.CategoryId);
				Assert.Equal(updatedProduct.Length, result.Details.Length);
				Assert.Equal(updatedProduct.Height, result.Details.Height);
		
		}
		[Fact]
		public void NotProperDataName_UpdateProduct_ThrowsException()
		{
			// Arrange			
			var details = new ProductDetail
			{
				//Id = 2,
				ProductId = 2,
				Height = 1,
				Width = 1,
				Length = 1,
				Weight = 1,
				Description = "Test"
			};
			var updatingProduct = new Product
			{
				Id = 2,
				Name = "Test",
				CategoryId = 1,
				SKU = "dede",
				IsDeleted = false,
				AddedItemAd = new DateTime(2024, 2, 2),
				Details = details
			};
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var updatedProduct = new AddProductDTO
			{
				Id = 2,
				//Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				//DetailsId = 2,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",
			};			
			var e = Assert.Throws<ValidationException>(()=> _productService.UpdateProduct(updatedProduct));
				Assert.Contains("Uzupełnij dane - nazwa", e.Message);							
		}
		[Fact]
		public void NotProperDataLength_UpdateProduct_ThrowsException()
		{
			// Arrange			
			var details = new ProductDetail
			{
				//Id = 3,
				ProductId = 3,
				Height = 1,
				Width = 1,
				Length = 1,
				Weight = 1,
				Description = "Test"
			};
			var updatingProduct = new Product
			{
				Id = 3,
				Name = "Test",
				CategoryId = 1,
				SKU = "dede",
				IsDeleted = false,
				AddedItemAd = new DateTime(2024, 2, 2),
				Details = details
			};
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var updatedProduct = new AddProductDTO
			{
				Id = 3,
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				//DetailsId = 3,
				Height = 10,
				Weight = 10,
				Width = 10,
				//Length = 10,
				Description = "TestOk",
			};
			var e = Assert.Throws<ValidationException>(() => _productService.UpdateProduct(updatedProduct));
			Assert.Contains("Uzupełnij dane - długość", e.Message);
		}
		[Fact]
		public async Task ProperData_UpdateProductAsync_ChangeData()
		{
			// Arrange			
			var details = new ProductDetail
			{
				//Id = 4,
				ProductId = 4,
				Height = 1,
				Width = 1,
				Length = 1,
				Weight = 1,
				Description = "Test"
			};
			var updatingProduct = new Product
			{
				Id = 4,
				Name = "Test",
				CategoryId = 1,
				SKU = "dede",
				IsDeleted = false,
				AddedItemAd = new DateTime(2024, 2, 2),
				Details = details
			};
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act
			var updatedProduct = new AddProductDTO
			{
				Id = 4,
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				//DetailsId = 4,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",

			};
		
				await _productService.UpdateProductAsync(updatedProduct);
			
			//Assert
			
				var result = _context.Products
					.Include(d => d.Details)
					.FirstOrDefault(x => x.Id == updatingProduct.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedProduct.Name, result.Name);
				Assert.Equal(updatedProduct.SKU, result.SKU);
				Assert.Equal(updatedProduct.CategoryId, result.CategoryId);
				Assert.Equal(updatedProduct.Length, result.Details.Length);
				Assert.Equal(updatedProduct.Height, result.Details.Height);
			
		}
		[Fact]
		public async Task NotProperDataName_UpdateProductAsync_ThrowsException()
		{
			// Arrange			
			var details = new ProductDetail
			{
				//Id = 5,
				ProductId = 5,
				Height = 1,
				Width = 1,
				Length = 1,
				Weight = 1,
				Description = "Test"
			};
			var updatingProduct = new Product
			{
				Id = 5,
				Name = "Test",
				CategoryId = 1,
				SKU = "dede",
				IsDeleted = false,
				AddedItemAd = new DateTime(2024, 2, 2),
				Details = details
			};
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var updatedProduct = new AddProductDTO
			{
				Id = 5,
				//Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				//DetailsId = 5,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",

			};
				var e =await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdateProductAsync(updatedProduct));
				Assert.Contains("Uzupełnij dane - nazwa", e.Message);
			
		}
		[Fact]
		public async Task NotProperDataLength_UpdateProductAsync_ThrowsException()
		{
			// Arrange			
			var details = new ProductDetail
			{
				//Id = 6,
				ProductId = 6,
				Height = 1,
				Width = 1,
				Length = 1,
				Weight = 1,
				Description = "Test"
			};
			var updatingProduct = new Product
			{
				Id = 6,
				Name = "Test",
				CategoryId = 1,
				SKU = "dede",
				IsDeleted = false,
				AddedItemAd = new DateTime(2024, 2, 2),
				Details = details
			};
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var updatedProduct = new AddProductDTO
			{
				Id = 6,
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				//DetailsId = 6,
				Height = 10,
				Weight = 10,
				Width = 10,
				//Length = 10,
				Description = "TestOk",
			};

			var e = await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdateProductAsync(updatedProduct));

			Assert.Contains("Uzupełnij dane - długość", e.Message);
		}
	}
}
