using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductTestsRepo
{
	public class UpdateProductTests
	//: CommandTestBase
	{
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		//private readonly ProductRepo _productRepo;
		public UpdateProductTests() : base()
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("TestDatabase")
				.Options;

			//_productRepo = new ProductRepo(_context);
		}
		[Fact]
		public void UpdateProperData_UpdateProduct_ShouldUpdateProduct()
		{
			//Arrange
			var updatingProduct = new Product
			{
				Id = 10,
				Name = "Apple",
				SKU = "1234567890",
				CategoryId = 2
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Products.Add(updatingProduct);
			arrangeContext.SaveChanges();
			//Act
			var updatedProduct = new Product
			{
				Id = 10,
				Name = "Banana",
				SKU = "12344467890",
				CategoryId = 1
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new ProductRepo(actContext);
				repo.UpdateProduct(updatedProduct);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Products.FirstOrDefault(x => x.Id == updatedProduct.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedProduct.Name, result.Name);
				Assert.Equal(updatedProduct.SKU, result.SKU);
				Assert.Equal(updatedProduct.CategoryId, result.CategoryId);
			}
		}
		[Fact]
		public void ChangeOneDiemension_UpdateProduct_ShouldChangeD()
		{
			//Arrange
			var updatingProduct = new Product
			{
				Id = 20,
				Name = "Apple",
				SKU = "1234567890",
				CategoryId = 2,
				Details = new ProductDetails
				{
					Id = 10,
					Height = 200,
					Length = 200,
					Weight = 200,
					Width = 200,
					Description = "testD",
					ProductId = 20
				}
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Products.Add(updatingProduct);
			arrangeContext.SaveChanges();
			//Act
			var updatedProduct = new Product
			{
				Id = 20,
				Name = "Banana",
				SKU = "12344467890",
				CategoryId = 1,
				Details = new ProductDetails
				{
					Id = 10,
					Height = 500,
					Length = 200,
					Weight = 200,
					Width = 200,
					Description = "testD",
					ProductId = 20
				}
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new ProductRepo(actContext);
				repo.UpdateProduct(updatedProduct);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Products
					.Include(p => p.Details)
					.FirstOrDefault(x => x.Id == updatedProduct.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedProduct.Name, result.Name);
				Assert.Equal(updatedProduct.SKU, result.SKU);
				Assert.Equal(updatedProduct.CategoryId, result.CategoryId);
				Assert.Equal(updatedProduct.Details.Height, result.Details.Height);
			}
		}
		[Fact]
		public void UpdateProperDataNoName_UpdateProduct_ShouldUpdateProduct()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var updatingProduct = new Product
			{
				Id = 11,
				Name = "Apple",
				SKU = "1234567890",
				CategoryId = 2
			};
			arrangeContext.Products.Add(updatingProduct);
			arrangeContext.SaveChanges();
			//Act
			var updatedProduct = new Product
			{
				Id = 11,
				//Name = "Banana",
				SKU = "12344467890",
				CategoryId = 1
			};
			var resultBool = true;
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new ProductRepo(actContext);
				 repo.UpdateProduct(updatedProduct);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Products.FirstOrDefault(x => x.Id == updatedProduct.Id);
				Assert.NotNull(result);
				Assert.True(resultBool);
				Assert.Equal(updatingProduct.Name, result.Name);
				Assert.Equal(updatedProduct.SKU, result.SKU);
				Assert.Equal(updatedProduct.CategoryId, result.CategoryId);
			}
		}
		//[Fact]
		//public void UpdateNotProperId_UpdateProduct_ShouldUpdateProduct()
		//{
		//	//Arrange
		//	using var arrangeContext = new WerehouseDbContext(_contextOptions);
		//	var updatingProduct = new Product
		//	{
		//		Id = 12,
		//		Name = "Apple",
		//		SKU = "1234567890",
		//		CategoryId = 2
		//	};
		//	arrangeContext.Products.Add(updatingProduct);
		//	arrangeContext.SaveChanges();
		//	//Act
		//	var updatedProduct = new Product
		//	{
		//		Id = 100,
		//		Name = "Banana",
		//		SKU = "12344467890",
		//		CategoryId = 1
		//	};
		//	var resultBool = true;
		//	using (var actContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var repo = new ProductRepo(actContext);

		//		resultBool = repo.UpdateProduct(updatedProduct);
		//	}
		//	//Assert			
		//		Assert.False(resultBool);				
		//}
		//[Fact]
		//public void UpdateNullData_UpdateProduct_ShouldUpdateProduct()
		//{
		//	//Arrange
		//	using var arrangeContext = new WerehouseDbContext(_contextOptions);
		//	var updatingProduct = new Product
		//	{
		//		Id = 13,
		//		Name = "Apple",
		//		SKU = "1234567890",
		//		CategoryId = 2
		//	};
		//	arrangeContext.Products.Add(updatingProduct);
		//	arrangeContext.SaveChanges();
		//	//Act
		//	var updatedProduct = new Product
		//	{

		//	};
		//	var resultBool = true;
		//	using (var actContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var repo = new ProductRepo(actContext);

		//		resultBool = repo.UpdateProduct(updatedProduct);
		//	}
		//	//Assert			
		//	Assert.False(resultBool);			
		//}
	}
}
