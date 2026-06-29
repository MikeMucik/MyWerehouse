using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.CategoryTestsRepoSQLite
{
	public class AddDeleteCategoryTests : TestBase
	{
		private readonly CategoryRepo _categoryRepo;
		public AddDeleteCategoryTests(): base()
		{
			_categoryRepo = new CategoryRepo(DbContext);
		}
	
		[Fact]
		public void AddCategory_AddCategory_ShouldAddToList()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			//Act
			_categoryRepo.AddCategory(newCategory);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Categories.Find(newCategory.Id);
			Assert.NotNull(result);
			Assert.Equal(newCategory.Name, result.Name);
		}		
		[Fact]
		public void DeleteCategory_DeleteCategory_ShouldRemoveFromList()
		{
			//Arrange
			var category = new Category
			{
				Name = "CategoryName"
			};
			//Act
			_categoryRepo.AddCategory(category);
			DbContext.SaveChanges();
			var resultAdded = DbContext.Categories.Find(category.Id);
			Assert.NotNull(resultAdded);
			_categoryRepo.DeleteCategory(category);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Categories.Find(category.Id);
			Assert.Null(result);
		}		
		[Fact]
		public async Task SwithOffCategory_SwithOffCategoryAsync_ShouldHideFromList()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var idCategory = 1;
			//Act
			await _categoryRepo.SwitchOffCategoryAsync(idCategory);
			//Assert
			var result = DbContext.Categories.Find(idCategory);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
	}
}
