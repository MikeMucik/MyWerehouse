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

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	public class UpdateProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public void ProperData_UpdateProduct_ChangeData()
		{
			// Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var details = new ProductDetails
			{
				Id = 1,
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
			arrangeContext.ProductDetails.Add(details);
			arrangeContext.Products.Add(updatingProduct);
			arrangeContext.SaveChanges();
			//Act
			var updatedProduct = new AddProductDTO
			{
				Id = 1,
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted=false,
				AddedItemAd = DateTime.Now,
				DetailsId = 1,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",

			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _productRepo = new ProductRepo(actContext);
				var _productValidator = new AddProductDTOValidation();
				var _productService = new ProductService(_productRepo, _mapper, _productValidator);
				_productService.UpdateProduct(updatedProduct);				
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Products
					.Include(d=>d.Details)
					.FirstOrDefault(x => x.Id == updatingProduct.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedProduct.Name, result.Name);
				Assert.Equal(updatedProduct.SKU, result.SKU);
				Assert.Equal(updatedProduct.CategoryId, result.CategoryId);
				Assert.Equal(updatedProduct.Length, result.Details.Length);
				Assert.Equal(updatedProduct.Height, result.Details.Height);
			}
		}
		[Fact]
		public void NotProperDataName_UpdateProduct_ThrowsException()
		{
			// Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var details = new ProductDetails
			{
				Id = 2,
				ProductId = 1,
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
			arrangeContext.ProductDetails.Add(details);
			arrangeContext.Products.Add(updatingProduct);
			arrangeContext.SaveChanges();
			//Act&Assert
			var updatedProduct = new AddProductDTO
			{
				Id = 2,
				//Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				DetailsId = 2,
				Height = 10,
				Weight = 10,
				Width = 10,
				Length = 10,
				Description = "TestOk",

			};
			
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _productRepo = new ProductRepo(actContext);
				var _productValidator = new AddProductDTOValidation();
				var _productService = new ProductService(_productRepo, _mapper, _productValidator);
				var e = Assert.Throws<ValidationException>(()=> _productService.UpdateProduct(updatedProduct));
				Assert.Contains("Uzupełnij dane - nazwa", e.Message);				
			}			
		}
		[Fact]
		public void NotProperDataLength_UpdateProduct_ThrowsException()
		{
			// Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var details = new ProductDetails
			{
				Id = 3,
				ProductId = 1,
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
			arrangeContext.ProductDetails.Add(details);
			arrangeContext.Products.Add(updatingProduct);
			arrangeContext.SaveChanges();
			//Act&Assert
			var updatedProduct = new AddProductDTO
			{
				Id = 3,
				Name = "Testqw",
				CategoryId = 2,
				SKU = "q1233",
				IsDeleted = false,
				AddedItemAd = DateTime.Now,
				DetailsId = 3,
				Height = 10,
				Weight = 10,
				Width = 10,
				//Length = 10,
				Description = "TestOk",
			};
			using var actContext = new WerehouseDbContext(_contextOptions);
			var _productRepo = new ProductRepo(actContext);
			var _productValidator = new AddProductDTOValidation();
			var _productService = new ProductService(_productRepo, _mapper, _productValidator);
			var e = Assert.Throws<ValidationException>(() => _productService.UpdateProduct(updatedProduct));

			Assert.Contains("Uzupełnij dane - długość", e.Message);
		}
	}
}
