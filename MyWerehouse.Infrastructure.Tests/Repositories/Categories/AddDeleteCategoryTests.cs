using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using TestSupport;

namespace MyWerehouse.Infrastructure.Tests.Repositories.Categories
{
	public class AddDeleteCategoryTests : SqliteTestDatabase
	{
		private readonly CategoryRepo _categoryRepo;
		public AddDeleteCategoryTests() : base()
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
			using var newDbContext = CreateNewContext();
			var result = newDbContext.Categories.Find(newCategory.Id);
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
			_categoryRepo.AddCategory(category);
			DbContext.SaveChanges();
			//Act
			var resultAdded = DbContext.Categories.Find(category.Id);
			Assert.NotNull(resultAdded);
			_categoryRepo.DeleteCategory(category);
			DbContext.SaveChanges();
			//Assert
			using var newDbContext = CreateNewContext();
			var result = newDbContext.Categories.Find(category.Id);
			Assert.Null(result);
		}
		[Fact]
		public async Task SwitchOffCategoryAsync_ShouldMarkCategoryAsDeleted_ShouldHideFromList()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);
			DbContext.SaveChanges();
			var idCategory = newCategory.Id;
			//Act
			await _categoryRepo.SwitchOffCategoryAsync(idCategory, CancellationToken.None);
			DbContext.SaveChanges();
			//Assert
			await using var newDbContext = CreateNewContext();
			var result = newDbContext.Categories.Find(idCategory);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
	}
}
