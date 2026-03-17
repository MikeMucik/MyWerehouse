using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;
using Xunit;

namespace MyWerehouse.Test.IntegrationTestRepo.CategoryTestsRepoSQLite
{
	[Collection("QueryCollection")] 
	public class ViewCategoryTests
	{
		private readonly CategoryRepo _categoryRepo;
		private readonly QueryTestFixture _fixture;  

		public ViewCategoryTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_categoryRepo = new CategoryRepo(_fixture.DbContext);  
		}

		[Fact]
		public void ShowAllCategories_GetAllCategories_ReturnList()
		{
			// Arrange & Act
			var result = _categoryRepo.GetAllCategories(); 
			// Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count());  
		}

		[Fact]
		public async Task ShowCategoryById_GetCategoryByIdAsync()
		{
			// Arrange & Act
			var result =await _categoryRepo.GetCategoryByIdAsync(1);
			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
		}

		[Fact]
		public async Task ShowCategoryByName_GetCategoryByNameAsync()
		{
			// Arrange & Act
			var result = await _categoryRepo.GetCategoryByNameAsync("TestCategory");
			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
		}		
	}
}
