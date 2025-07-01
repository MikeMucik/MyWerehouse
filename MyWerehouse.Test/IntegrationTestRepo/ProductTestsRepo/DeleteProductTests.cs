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
	public class DeleteProductTests : CommandTestBase
	{
		private readonly ProductRepo _productRepo;
		public DeleteProductTests(): base()
		{
			_productRepo = new ProductRepo(_context);
		}
		//[Fact]
		//public void RemoveProduct_DeleteProduct_ShouldRemoveFromCollection()
		//{
		//	//Arrange
		//	var product = new Product
		//	{
		//		Id = 1,
		//		Name = "Banana",
		//		SKU = "1234567890",
		//		CategoryId = 1,
		//		CartonsPerPallet = 56,
		//	};
		//	_context.Products.Add(product);
		//	_context.SaveChanges();
		//	//Act
		//	_productRepo.DeleteProductById(product.Id);
		//	//Assert
		//	var productDeleted = _context.Products.Find(product.Id);
		//	Assert.Null(productDeleted);
			
		//}
		//[Fact]
		//public async Task RemoveProduct_DeleteProductAsync_ShouldRemoveFromCollection()
		//{
		//	//Arrange
		//	var product = new Product
		//	{
		//		Id = 1,
		//		Name = "Banana",
		//		SKU = "1234567890",
		//		CategoryId = 1,
		//		CartonsPerPallet = 56,
		//	};
		//	_context.Products.Add(product);
		//	_context.SaveChanges();
		//	//Act
		//	await _productRepo.DeleteProductByIdAsync(product.Id);
		//	//Assert
		//	var productDeleted = _context.Products.Find(product.Id);
		//	Assert.Null(productDeleted);

		//}
		//[Fact]
		//public void SwithOffProduct_SwithOffProduct_ShouldHideProduct()
		//{
		//	//Arrange
		//	var product = new Product
		//	{
		//		Id = 1,
		//		Name = "Banana",
		//		SKU = "1234567890",
		//		CategoryId = 1,
		//		CartonsPerPallet = 56,
		//	};
		//	_context.Products.Add(product);
		//	_context.SaveChanges();
		//	//Act
		//	_productRepo.SwitchOffProduct(product.Id);
		//	//Assert
		//	var productDeleted = _context.Products.Find(product.Id);			
		//	Assert.True(productDeleted.IsDeleted);			
		//}
		//[Fact]
		//public async Task SwithOffProduct_SwithOffProductAsync_ShouldHideProduct()
		//{
		//	//Arrange
		//	var product = new Product
		//	{
		//		Id = 1,
		//		Name = "Banana",
		//		SKU = "1234567890",
		//		CategoryId = 1,
		//		CartonsPerPallet = 56,
		//	};
		//	_context.Products.Add(product);
		//	_context.SaveChanges();
		//	//Act
		//	await _productRepo.SwitchOffProductAsync(product.Id);
		//	//Assert
		//	var productDeleted = _context.Products.Find(product.Id);
		//	Assert.True(productDeleted.IsDeleted);
		//}
	}
}
