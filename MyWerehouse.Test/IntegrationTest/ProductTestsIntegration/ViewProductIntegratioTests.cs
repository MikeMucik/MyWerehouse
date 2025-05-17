using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewProductIntegratioTests(QuerryTestFixture fixture) :ProductIntegrationView(fixture)
	{
		[Fact]
		public void ShowProductDetails_DetailsOfProduct_ReturnData()
		{
			//Arrange
			var productId = 10;
			//Act
			var result = _productService.DetailsOfProduct(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("Test", result.Name);
			Assert.Equal("TestDetails", result.Description);
		}
		[Fact]
		public void ShowProductDetailsBadId_DetailsOfProduct_ThrowException()
		{
			//Arrange
			var productId = 90;
			//Act
			var result = _productService.DetailsOfProduct(productId);
			//Assert
			Assert.Null(result);			
		}
		[Fact]
		public void ShowProduct_GetProductToEdit_ReturnAddProductDTO()
		{
			//Arrange
			var productId = 10;
			//Act
			var result = _productService.GetProductToEdit(productId);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<AddProductDTO>(result);
		}
	}
}
