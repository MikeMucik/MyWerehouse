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
			Assert.Equal(2, result.Count());
		}
	}
}
