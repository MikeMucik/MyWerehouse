using MyWerehouse.Infrastructure.Persistence.Repositories;
using TestSupport;
using TestSupport.Seeders.Scenarios;

namespace MyWerehouse.Infrastructure.Tests.Repositories.Categories
{
	public class ViewCategoryTestsRepo : SqliteTestDatabase
	{
		private readonly CategoryRepo _categoryRepo;

		public ViewCategoryTestsRepo()
		{			
			SeederForCategory.SeedDatabase(DbContext);
			_categoryRepo = new CategoryRepo(DbContext);
		}


		[Fact]
		public async Task ShowCategoryById_GetCategoryByIdAsync()
		{
			// Arrange & Act
			var result = await _categoryRepo.GetCategoryByIdAsync(1, CancellationToken.None);
			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
		}

		[Fact]
		public async Task ShowCategoryByName_GetCategoryByNameAsync()
		{
			// Arrange & Act
			var result = await _categoryRepo.GetCategoryByNameAsync("TestCategory", CancellationToken.None);
			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
		}
		[Fact]
		public async Task ReturnNull_GetCategoryByIdAsync_WhenCategorySwitchOff()
		{
			// Arrange & Act
			var result = await _categoryRepo.GetCategoryByIdAsync(4, CancellationToken.None);
			// Assert
			Assert.Null(result);
		}
		[Fact]
		public async Task ReturnNull_GetCategoryByNameAsync_WhenCategorySwitchOff()
		{
			// Arrange & Act
			var result = await _categoryRepo.GetCategoryByNameAsync("SwitchOff", CancellationToken.None);
			// Assert
			Assert.Null(result);
		}
	}
}
