using Moq;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Tests.Categories.Service
{
	public class GetCategoryTests : CategoryServiceCommand
	{
		[Fact]
		public async Task GetCategoryByIdAsync_ReturnData_WhenCategoryExist()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "TestCategory"
			};
			var id = 1;
			_categoryRepo.Setup(repo => repo.GetCategoryByIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(category);
			//Act
			var result = await _categoryService.GetCategoryByIdAsync(id, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.True(result.Result.Name == category.Name);
			Assert.IsType<AppResult<CategoryViewDTO>>(result);
			Assert.IsType<CategoryViewDTO>(result.Result);
			_categoryRepo.Verify(work => work.GetCategoryByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
		}
		[Fact]
		public async Task GetCategoryByIdAsync_ReturnNoData_WhenCategoryNotExist()
		{
			//Arrange
			var id = 1;
			_categoryRepo.Setup(repo => repo.GetCategoryByIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Category?)null);
			//Act
			var result = await _categoryService.GetCategoryByIdAsync(id, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.True(result.ErrorType == Domain.Common.ValueObject.ErrorType.NotFound);
			Assert.IsType<AppResult<CategoryViewDTO>>(result);
			Assert.Contains("Category was not found.", result.Error);
			_categoryRepo.Verify(work => work.GetCategoryByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
