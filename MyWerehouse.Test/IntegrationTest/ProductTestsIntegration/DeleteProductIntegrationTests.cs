using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	public class DeleteProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public void HideProduct_DeleteProduct_ChangeNotActive()
		{
			//Arrange
			var productId = 10;
			//Act
			_productService.DeleteProduct(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.NotNull(product);
			Assert.True(product.IsDeleted);
		}
		[Fact]
		public void RemoveProduct_DeleteProduct_DeleteFromList()
		{
			//Arrange
			var productId = 989;
			//Act
			_productService.DeleteProduct(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.Null(product);
		}
		[Fact]
		public void RemoveNotProperIdProduct_DeleteProduct_ThrowException()
		{
			//Arrange
			var productId = 9891;
			//Act&Assert
			var e = Assert.Throws<InvalidDataException>(() => _productService.DeleteProduct(productId));
			Assert.NotNull(e);
			Assert.Contains("Brak produktu o tym numerze", e.Message);
		}
		[Fact]
		public async Task HideProduct_DeleteProductAsync_ChangeNotActive()
		{
			//Arrange
			var productId = 10;
			//Act
			await _productService.DeleteProductAsync(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.NotNull(product);
			Assert.True(product.IsDeleted);
		}
		[Fact]
		public async Task RemoveProduct_DeleteProductAsync_DeleteFromList()
		{
			//Arrange
			var productId = 989;
			//Act
			await _productService.DeleteProductAsync(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.Null(product);
		}
		[Fact]
		public async Task RemoveNotProperIdProduct_DeleteProductAsync_ThrowException()
		{
			//Arrange
			var productId = 9891;
			//Act&Assert
			var e =await Assert.ThrowsAsync<InvalidDataException>(() => _productService.DeleteProductAsync(productId));
			Assert.NotNull(e);
			Assert.Contains("Brak produktu o tym numerze", e.Message);
		}
	}
}
