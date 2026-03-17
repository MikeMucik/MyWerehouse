using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.CategoryTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewCategoryIntegrationTests : CommandTestBase
	{
		private readonly CategoryService _categoryService;
		private readonly CategoryRepo _categoryRepo;
		public ViewCategoryIntegrationTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_categoryRepo = new CategoryRepo(_context);
			var _productRepo = new ProductRepo(_context);
			_categoryService = new CategoryService(_categoryRepo, _mapper);
		}
		[Fact]
		public async Task ShowAllCategories_GetCategoriesAsync_ReturnList()
		{
			//Arrange
			var pageSize = 5;
			var pagenumber = 1;
			//Act
			var result = await _categoryService.GetCategoriesAsync(pageSize, pagenumber);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Result.Categories.Count);
		}
	}
}
