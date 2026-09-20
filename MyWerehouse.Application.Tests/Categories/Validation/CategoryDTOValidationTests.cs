using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using MyWerehouse.Application.ViewModels.CategoryModels;

namespace MyWerehouse.Application.Tests.Categories.Validation
{
	public class CategoryDTOValidationTests
	{
		[Fact]
		public void AddCategory_ShouldNotReturnValidationError_WhenProperData()
		{
			//Arrange
			var validator = new ViewModels.CategoryModels.CategoryDTOValidation();
			var category = new CategoryDTO { Name = "test" };
			//Act&Assert
			validator.TestValidate(category).ShouldNotHaveAnyValidationErrors();
		}
		
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void AddCategory_ShouldReturnValidationError(string? category)
		{
			var validator = new ViewModels.CategoryModels.CategoryDTOValidation();
			var categoryDTO = new CategoryDTO { Name = category !};
			//Act&Assert
			validator.TestValidate(categoryDTO).ShouldHaveValidationErrorFor(c=>c.Name);
		}
	}
}
