using FluentValidation;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Infrastructure.Persistence.ReadServices;
using MyWerehouse.Test.InMemoryDatabase.Common;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.CategoryTestsIntegration
{
	[Collection("QueryCollectionInMemory")]
	public class ViewCategoryIntegrationTests : CommandTestBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly CategoryService _categoryService;
		private readonly CategoryRepo _categoryRepo;
		private readonly IProductRepo _productRepo;
		private readonly IValidator<CategoryDTO> _validator;
		private readonly ICategoryReadService _categoryReadService;

		public ViewCategoryIntegrationTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_unitOfWork = new UnitOfWork(_context);
			_categoryRepo = new CategoryRepo(_context);
			_productRepo = new ProductRepo(_context);
			_validator = new CategoryDTOValidation();
			_categoryReadService = new CategoryReadService(_context);
			_categoryService = new CategoryService(_unitOfWork, _categoryRepo, _productRepo, _validator,_categoryReadService);
		}
		[Fact]
		public async Task GetCategoriesAsync_ShouldReturnCategories_WhenDataExist()
		{
			//Arrange
			var pageSize = 5;
			var pagenumber = 1;
			//Act
			var result = await _categoryService.GetCategoriesAsync(pagenumber, pageSize, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(3, result.Result.Items.Count);
		}

		[Fact]
		public async Task GetCategoriesAsync_ShouldReturnCorrectPagination_WhenTwelveCategoriesExist()
		{
			// Arrange - baza startuje z trzema kategoriami, więc dokładamy dziewięć.
			var additionalCategories = Enumerable.Range(1, 9)
				.Select(number => new Category
				{
					Name = $"Pagination category {number:D2}",
					IsDeleted = false
				})
				.ToList();

			foreach (var category in additionalCategories)
			{
				_categoryRepo.AddCategory(category);
			}

			await _unitOfWork.SaveChangesAsync(CancellationToken.None);

			try
			{
				// Act
				var firstPage = await _categoryService.GetCategoriesAsync(1, 5, CancellationToken.None);
				var secondPage = await _categoryService.GetCategoriesAsync(2, 5, CancellationToken.None);
				var thirdPage = await _categoryService.GetCategoriesAsync(3, 5, CancellationToken.None);

				// Assert
				Assert.True(firstPage.IsSuccess);
				Assert.True(secondPage.IsSuccess);
				Assert.True(thirdPage.IsSuccess);
				Assert.NotNull(firstPage.Result);
				Assert.NotNull(secondPage.Result);
				Assert.NotNull(thirdPage.Result);

				Assert.Equal(5, firstPage.Result.Items.Count);//??
				Assert.Equal(5, secondPage.Result.Items.Count);
				Assert.Equal(2, thirdPage.Result.Items.Count);
				Assert.Equal(12, firstPage.Result.TotalCount);//??
				Assert.Equal(3, firstPage.Result.TotalPages);
				Assert.True(firstPage.Result.HasNext);
				Assert.False(firstPage.Result.HasPrevious);
				Assert.False(thirdPage.Result.HasNext);
				Assert.True(thirdPage.Result.HasPrevious);
			}
			finally
			{
				foreach (var category in additionalCategories)
				{
					_categoryRepo.DeleteCategory(category);
				}

				await _unitOfWork.SaveChangesAsync(CancellationToken.None);
			}
		}
	}
}
