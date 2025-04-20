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
		[Fact]
		public void RemoveProduct_DeleteProduct_ShouldRemoveFromCollection()
		{
			//Arrange
			var product = new Product
			{
				Id = 11
			};
			//Act
			var result = _productRepo.DeleteProductById(product.Id);
			//Assert
			var productDeleted = _context.Products.Find(product.Id);
			Assert.Null(productDeleted);
			Assert.True(result);
		}
		[Fact]
		public void RemoveNotExixstingProduct_DeleteProduct_Should()
		{
			//Arrange
			var product = new Product
			{
				Id = 111
			};
			//Act
			var result = _productRepo.DeleteProductById(product.Id);
			//Assert
			var productDeleted = _context.Products.Find(product.Id);
			Assert.Null(productDeleted);
			Assert.False(result);
		}
	}
}
