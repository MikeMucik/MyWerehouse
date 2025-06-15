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
	public class AddProductTests : CommandTestBase
	{
		private readonly ProductRepo _productRepo;
		public AddProductTests() : base()
		{
			_productRepo = new ProductRepo(_context);
		}
		[Fact]
		public void AddProperData_AddProduct_ShouldAddToCollection()
		{
			//Arrange
			var productRepo = new Product
			{			
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			//Act
			var result = _productRepo.AddProduct(productRepo);
			//Assert			
			var fullResult = _context.Products.FirstOrDefault(p => p.Name == productRepo.Name);
			Assert.Equal("1234567890", fullResult.SKU);
		}
		[Fact]
		public async Task AddProperData_AddProductAsync_ShouldAddToCollection()
		{
			//Arrange
			var productRepo = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			//Act
			var result =await _productRepo.AddProductAsync(productRepo);
			//Assert			
			var fullResult = _context.Products.FirstOrDefault(p => p.Name == productRepo.Name);
			Assert.Equal("1234567890", fullResult.SKU);
		}
		[Fact]
		public void AddProperDataWithDetails_AddProduct_ShouldAddToCollection()
		{
			//Arrange
			var details = new ProductDetails
			{
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			var productRepo = new Product
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Details = details,
				CartonsPerPallet = 56,
			};
			//Act
			var result = _productRepo.AddProduct(productRepo);
			//Assert			
			var fullResult = _context.Products.FirstOrDefault(p => p.Name == productRepo.Name);
			Assert.Equal("666666", fullResult.SKU);
			Assert.Equal(100, fullResult.Details.Length);
		}
		[Fact]
		public async Task AddProperDataWithDetails_AddProductAsync_ShouldAddToCollection()
		{
			//Arrange
			var details = new ProductDetails
			{
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			var productRepo = new Product
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Details = details,
				CartonsPerPallet = 56,
			};
			//Act
			var result =await _productRepo.AddProductAsync(productRepo);
			//Assert			
			var fullResult = _context.Products.FirstOrDefault(p => p.Name == productRepo.Name);
			Assert.Equal("666666", fullResult.SKU);
			Assert.Equal(100, fullResult.Details.Length);
		}
	}
}
