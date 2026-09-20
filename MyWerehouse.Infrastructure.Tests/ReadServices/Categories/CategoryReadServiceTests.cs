using MyWerehouse.Infrastructure.Persistence.ReadServices;
using TestSupport;
using TestSupport.Seeders.Scenarios;

namespace MyWerehouse.Infrastructure.Tests.ReadServices.Categories
{
	public class CategoryReadServiceTests : SqliteTestDatabase
	{
		private readonly CategoryReadService _categoryReadService;

		public CategoryReadServiceTests()
		{
			SeederForCategory.SeedDatabase(DbContext);
			_categoryReadService = new CategoryReadService(DbContext);
		}

		[Fact]
		public async Task GetCategoriesAsync_ShouldReturnRequestedPageOrderedByName()
		{
			// Act
			var result = await _categoryReadService.GetCategoriesAsync(
				pageNumber: 1,
				pageSize: 2,
				CancellationToken.None);

			// Assert
			Assert.Equal(3, result.TotalCount);
			Assert.Equal(1, result.CurrentPage);
			Assert.Equal(2, result.PageSize);
			Assert.Equal(2, result.Items.Count);
			Assert.Equal(
				["TestCategory", "TestCategory1"],
				result.Items.Select(category => category.Name));
		}
	}
}
