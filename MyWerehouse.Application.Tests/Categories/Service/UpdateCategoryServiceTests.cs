using FluentValidation;
using Moq;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Tests.Categories.Service
{
	public class UpdateCategoryServiceTests : CategoryServiceCommand
	{
		[Fact]
		public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenNameIsAvailable()
		{
			//Arrange
			var category = new Category { Id = 3, Name = "OldCategory" };
			var categoryDTO = new CategoryDTO { Name = "NewCategory" };
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
				.ReturnsAsync(category);
			_categoryRepo
				.Setup(repo => repo.GetCategoryByNameAsync(categoryDTO.Name, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Category?)null);
			_unitOfWork
				.Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(1);

			//Act
			var result = await _categoryService.UpdateCategoryAsync(category.Id, categoryDTO, CancellationToken.None);

			//Assert
			Assert.True(result.IsSuccess);
			Assert.Equal("Category updated.", result.Message);
			Assert.Equal(categoryDTO.Name, category.Name);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task UpdateCategoryAsync_ShouldSucceed_WhenNameBelongsToSameCategory()
		{
			//Arrange
			var category = new Category { Id = 3, Name = "TestCategory" };
			var categoryDTO = new CategoryDTO { Name = category.Name };
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
				.ReturnsAsync(category);
			_categoryRepo
				.Setup(repo => repo.GetCategoryByNameAsync(categoryDTO.Name, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new Category { Id = category.Id, Name = category.Name });
			_unitOfWork
				.Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(1);

			//Act
			var result = await _categoryService.UpdateCategoryAsync(category.Id, categoryDTO, CancellationToken.None);

			//Assert
			Assert.True(result.IsSuccess);
			Assert.Equal(categoryDTO.Name, category.Name);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task UpdateCategoryAsync_ShouldReturnConflict_WhenNameBelongsToAnotherCategory()
		{
			//Arrange
			var category = new Category { Id = 3, Name = "OldCategory" };
			var categoryDTO = new CategoryDTO { Name = "TakenCategory" };
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
				.ReturnsAsync(category);
			_categoryRepo
				.Setup(repo => repo.GetCategoryByNameAsync(categoryDTO.Name, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new Category { Id = 4, Name = categoryDTO.Name });

			//Act
			var result = await _categoryService.UpdateCategoryAsync(category.Id, categoryDTO, CancellationToken.None);

			//Assert
			Assert.False(result.IsSuccess);
			Assert.Equal(ErrorType.Conflict, result.ErrorType);
			Assert.Equal("OldCategory", category.Name);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public async Task UpdateCategoryAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
		{
			//Arrange
			var categoryId = 3;
			var categoryDTO = new CategoryDTO { Name = "NewCategory" };
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Category?)null);

			//Act
			var result = await _categoryService.UpdateCategoryAsync(categoryId, categoryDTO, CancellationToken.None);

			//Assert
			Assert.False(result.IsSuccess);
			Assert.Equal(ErrorType.NotFound, result.ErrorType);
			Assert.Equal($"Category {categoryId} was not found.", result.Error);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public async Task UpdateCategoryAsync_ShouldThrowValidationException_WhenInvalidInput()
		{
			//Arrange
			var category = new Category { Id = 3, Name = "OldCategory" };
			var categoryDTO = new CategoryDTO { Name = "" };
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
				.ReturnsAsync(category);

			//Act & Assert
			var exception = await Assert.ThrowsAsync<ValidationException>(() =>
				_categoryService.UpdateCategoryAsync(category.Id, categoryDTO, CancellationToken.None));
			Assert.Contains(exception.Errors, error => error.PropertyName == nameof(CategoryDTO.Name));
			Assert.Equal("OldCategory", category.Name);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		}
	}
}
