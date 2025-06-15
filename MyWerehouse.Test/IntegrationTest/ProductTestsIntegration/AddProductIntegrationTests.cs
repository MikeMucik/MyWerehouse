using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.ViewModels.ProductModels;

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	public class AddProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public void NewProductProperData_AddNewProduct_AddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act			
			var result = _productService.AddProduct(productNew);
			//Assert
			Assert.NotNull(result);
			var product = _context.Products.Find(result);
			Assert.NotNull(product);
			Assert.Equal(productNew.Length, product.Details.Length);
		}
		[Fact]
		public void NewProductInvalidDataHeight_AddNewProduct_NoAddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				//Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act&Assert
			var ex = Assert.Throws<ValidationException>(() => _productService.AddProduct(productNew));
			Assert.Contains("Uzupełnij dane - wysokość", ex.Message);			
		}
		[Fact]
		public void NewProductInvalidDataName_AddNewProduct_NoAddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act&Assert
			var ex = Assert.Throws<InvalidDataException>(() => _productService.AddProduct(productNew));
			Assert.Contains("Produkt o tej nazwie już istnieje.", ex.Message);
		}
		[Fact]
		public void NewProductInvalidDataSKU_AddNewProduct_NoAddedToCollection()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				//SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act&Assert
			var ex = Assert.Throws<ValidationException>(() => _productService.AddProduct(productNew));
			Assert.Contains("Uzupełnij dane - SKU", ex.Message);
		}
	}
}
