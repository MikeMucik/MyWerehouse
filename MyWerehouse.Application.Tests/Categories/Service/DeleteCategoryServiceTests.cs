using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Tests.Categories.Service
{
	public class DeleteCategoryServiceTests : CategoryServiceCommand
	{
		[Fact]
		public async Task DeleteCategoryAsync_ShouldNotDeleteCategory_WhenCategoryDoesNotExist()
		{
			//Arrange
			var categoryId = 3;
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Category?)null);			
			//Act
			var result = await _categoryService.DeleteCategoryAsync(categoryId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains($"Category {categoryId} was not found.", result.Error);
			_categoryRepo.Verify(repo => repo.GetCategoryByIdAsync(
				categoryId, It.IsAny<CancellationToken>() ), Times.Once);
			_categoryRepo.Verify(repo => repo.DeleteCategory(
				It.Is<Category>(category => category.Id == categoryId)), Times.Never);
			_categoryRepo.Verify(repo => repo.SwitchOffCategoryAsync(
				categoryId, It.IsAny<CancellationToken>()), Times.Never);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		}
		[Fact]
		public async Task DeleteCategoryAsync_ShouldDeleteCategory_WhenCategoryHasNoProduct()
		{
			//Arrange
			var category = new Category
			{
				Id = 3,
				Name = "TestCategory"
			};
			var categoryId = 3;
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
				.ReturnsAsync(category);
			_productRepo
				.Setup(repo=>repo.HasProductsInCategory(categoryId, It.IsAny<CancellationToken>()))
				.ReturnsAsync(false);
			//Act
			var result = await _categoryService.DeleteCategoryAsync(categoryId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("Category deleted.", result.Message);
			_categoryRepo.Verify(repo =>repo.DeleteCategory(
				It.Is<Category>(category=>category.Id == categoryId)), Times.Once);
			_unitOfWork.Verify(work=>work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}
		[Fact]
		public async Task DeleteCategoryAsync_ShouldSwitchOffCategory_WhenCategoryHasProduct()
		{
			//Arrange
			var category = new Category
			{
				Id = 3,
				Name = "TestCategory"
			};
			var categoryId = 3;
			_categoryRepo
				.Setup(repo => repo.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
				.ReturnsAsync(category);
			_productRepo
				.Setup(repo => repo.HasProductsInCategory(categoryId, It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);
			//Act
			var result = await _categoryService.DeleteCategoryAsync(categoryId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("Category disabled.", result.Message);
			_categoryRepo.Verify(repo => repo.SwitchOffCategoryAsync(
				categoryId, It.IsAny<CancellationToken>()), Times.Once);
			_categoryRepo.Verify(repo => repo.DeleteCategory(
			It.Is<Category>(category => category.Id == categoryId)), Times.Never);
			_unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
