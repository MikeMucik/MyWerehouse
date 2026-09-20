using FluentValidation.TestHelper;
using MyWerehouse.Application.ViewModels.ProductModels;

namespace MyWerehouse.Application.Tests.Products.Validation
{
	public class AddProductDTOTests
	{
		[Fact]
		public void AddProductProperData_ShouldNotReturnValidationError()
		{
			//Arrange
			var validator = new AddProductDTOValidation();			
			var product = new CreateProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
				CartonsPerPallet =56
			};
			//Act&Assert
			validator.TestValidate(product).ShouldNotHaveAnyValidationErrors();
		}
		[Fact]
		public void AddProductNotProperData_ShouldReturnValidationError()
		{
			//Arrange
			var validator = new AddProductDTOValidation();
			var product = new CreateProductDTO
			{
				Name = "",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
				CartonsPerPallet =56
			};
			//Act&Assert
			validator.TestValidate(product).ShouldHaveValidationErrorFor(nameof(EditProductDTO.Name));
		}
		[Fact]
		public void AddProductNotProperDataWidth_ShouldReturnValidationError()
		{
			//Arrange
			var validator = new AddProductDTOValidation();
			var product = new CreateProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				//Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
				CartonsPerPallet= 56
			};
			//Act&Assert
			validator.TestValidate(product).ShouldHaveValidationErrorFor(nameof(EditProductDTO.Length));
		}
	}
}
