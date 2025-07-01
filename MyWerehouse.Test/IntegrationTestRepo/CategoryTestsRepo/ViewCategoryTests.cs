using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.CategoryTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewCategoryTests
	{
		private readonly CategoryRepo _categoryRepo;
		public ViewCategoryTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_categoryRepo = new CategoryRepo(_context);
		}
		[Fact]
		public void ShowAllCategories_GetAllCategories_ReturnList()
		{
			//Arrang&Act
			var result = _categoryRepo.GetAllCategories();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count());
		}
		[Fact]
		public void ShowOneCategory_GetAllCategories_Return()
		{
			//Arrange
			var id = 1;
			//Arrang&Act
			var result = _categoryRepo.GetCategoryById(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("TestCategory", result.Name);
		}
		[Fact]
		public void ShowOneCategoryName_GetAllCategories_Return()
		{
			//Arrange
			var name = "TestCategory";
			//Arrang&Act
			var result = _categoryRepo.GetCategoryByName(name);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
		}
	}
}
