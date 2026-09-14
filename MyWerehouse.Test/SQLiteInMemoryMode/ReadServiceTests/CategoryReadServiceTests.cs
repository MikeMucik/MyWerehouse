using MyWerehouse.Server.ServicesToInfrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class CategoryReadServiceTests : TestBase
	{
		private readonly CategoryReadService _categoryReadService;

		public CategoryReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
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
