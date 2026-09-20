using FluentValidation;
using Moq;
using MyWerehouse.Application.Categories.Services;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Tests.Categories.Service
{
	public class AddCategoryServiceTests : CategoryServiceCommand
	{
		[Fact]
		public async Task AddCategory_ShouldAddCategory_WhenValidInput()
		{
			//Arrange			
			var categoryDTO = new CategoryDTO
			{
				Name = "newCategory"
			};
			_categoryRepo
				.Setup(repo => repo.GetCategoryByNameAsync(categoryDTO.Name, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Category?)null);
			_unitOfWork
				.Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(1);
			//Act
			var result = await _categoryService.AddCategoryAsync(categoryDTO, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("Category added.", result.Message);
			_categoryRepo.Verify(repo => repo.AddCategory(
				It.Is<Category>(category => category.Name == categoryDTO.Name)), Times.Once);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}
		[Fact]
		public async Task AddCategory_ShouldThrowValidationException_WhenInvalidInput()
		{
			//Arrange			
			var categoryDTO = new CategoryDTO
			{
				Name = ""
			};
			//Act & Assert
			var exception = await Assert.ThrowsAsync<ValidationException>(() =>
				_categoryService.AddCategoryAsync(categoryDTO, CancellationToken.None));
			Assert.Contains(exception.Errors, error => error.PropertyName == nameof(CategoryDTO.Name));
			_categoryRepo.Verify(repo => repo.AddCategory(It.IsAny<Category>()), Times.Never);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public async Task AddCategory_ShouldReturnConflict_WhenNameAlreadyExists()
		{
			//Arrange
			var categoryDTO = new CategoryDTO { Name = "existingCategory" };
			var existingCategory = new Category { Id = 1, Name = categoryDTO.Name };
			_categoryRepo
				.Setup(repo => repo.GetCategoryByNameAsync(categoryDTO.Name, It.IsAny<CancellationToken>()))
				.ReturnsAsync(existingCategory);

			//Act
			var result = await _categoryService.AddCategoryAsync(categoryDTO, CancellationToken.None);

			//Assert
			Assert.False(result.IsSuccess);
			Assert.Equal(ErrorType.Conflict, result.ErrorType);
			_categoryRepo.Verify(repo => repo.AddCategory(It.IsAny<Category>()), Times.Never);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		}
	}
}
