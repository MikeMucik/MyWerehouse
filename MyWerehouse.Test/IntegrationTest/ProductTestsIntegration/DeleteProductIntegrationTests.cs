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
		public void HideProduct_DeleteProduct_ChangeNotActice()
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
		public void RemoveProduct_DeleteProduct_ChangeNotActice()
		{
			//Arrange
			var productId = 989;
			//Act
			_productService.DeleteProduct(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.Null(product);
		}
	}
}
