using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.CategoryTestsIntegration
{
	[Collection("QueryCollectionInMemory")]
	public class ViewCategoryIntegrationTests : CommandTestBase
	{
		private readonly CategoryService _categoryService;
		private readonly CategoryRepo _categoryRepo;
		public ViewCategoryIntegrationTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_categoryRepo = new CategoryRepo(_context);
			var _productRepo = new ProductRepo(_context);
			_categoryService = new CategoryService(_categoryRepo, _mapper, _context);
		}
		[Fact]
		public async Task GetCategoriesAsync_ShouldReturnCategories_WhenDataExist()
		{
			//Arrange
			var pageSize = 5;
			var pagenumber = 1;
			var ct = CancellationToken.None;
			//Act
			var result = await _categoryService.GetCategoriesAsync(pageSize, pagenumber, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(3, result.Result.Items.Count);
		}
	}
}
