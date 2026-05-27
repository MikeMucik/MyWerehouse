using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Products.ProductsExceptions;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ProductTestsIntegration
{
	public class UpdateProductIntegrationTests : ProductIntegrationCommand
	{
		//private class SeedFor

		[Fact]
		public async Task UpdateProductAsync_ChangeData_WhenProperData()
		{
			// Arrange		
			var category = new Category
			{
				Name = "qwe"
			};
			var category1 = new Category
			{
				Name = "qwe111"
			};
			_context.Categories.AddRange(category, category1);
			_context.SaveChanges();
			var updatingProduct = Product.Create("Test", "dede", 1, 56);			
			var details = ProductDetail.CreateDetails(updatingProduct.Id, 1, 1, 1, 1, "Test");			
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act
			var id = updatingProduct.Id;
			var updatedProduct = new EditProductDTO
			{
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",
				CartonsPerPallet =56
			};
			await _productService.UpdateProductAsync(id, updatedProduct);
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
		public async Task UpdateProductAsync_ThrowsException_WhenNotProperDataName()
		{
			// Arrange			
			var updatingProduct = Product.Create("Test", "dede", 1, 56);			
			var details = ProductDetail.CreateDetails(updatingProduct.Id, 1, 1, 1, 1, "Test");
			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var id = updatingProduct.Id;
			var updatedProduct = new EditProductDTO
			{
				//Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",
				CartonsPerPallet =56

			};
			var e = await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdateProductAsync(id, updatedProduct));
			Assert.Contains("Uzupełnij dane - nazwa", e.Message);

		}
		[Fact]
		public async Task UpdateProductAsync_ThrowsValidationException_WhenNoDataLength()
		{
			// Arrange			
			var updatingProduct = Product.Create("Test", "dede", 1, 56);			
			var details = ProductDetail.CreateDetails(updatingProduct.Id, 1, 1, 1, 1, "Test");

			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var id = updatingProduct.Id;
			var updatedProduct = new EditProductDTO
			{
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				Height = 10,
				Weight = 10,
				Width = 10,
				//Length = 10,
				Description = "TestOk",
				CartonsPerPallet =56,
				
			};
			var e = await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdateProductAsync(id, updatedProduct));

			Assert.Contains("Uzupełnij dane - długość", e.Message);
		}
		[Fact]
		public async Task UpdateProductAsync_ThrowsDomainException_WhenNotProperDataLength()
		{
			// Arrange		
			var category = new Category
			{
				Name = "qwe"
			}
			;
			var category1 = new Category
			{
				Name = "qwe111"
			};
			_context.Categories.AddRange(category, category1);
			_context.SaveChanges();
			var updatingProduct = Product.Create("Test", "dede", 1, 56);
			var details = ProductDetail.CreateDetails(updatingProduct.Id, 1, 1, 1, 1, "Test");

			_context.ProductDetails.Add(details);
			_context.Products.Add(updatingProduct);
			_context.SaveChanges();
			//Act&Assert
			var id = updatingProduct.Id;
			var updatedProduct = new EditProductDTO
			{
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 610,
				Description = "TestOk",
				CartonsPerPallet =56
			};
			var e = await Assert.ThrowsAsync<WrongLengthProductDomainException>(() => _productService.UpdateProductAsync(id, updatedProduct));

			Assert.Contains("Not corect size of length(range: 1-120cm).", e.Message);
		}
	}
}
