using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.CategoryTestsRepo
{
	public class AddUpdateDeleteCategoryTests : CommandTestBase
	{
		private readonly CategoryRepo _categoryRepo;
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public AddUpdateDeleteCategoryTests() : base()
		{
			_categoryRepo = new CategoryRepo(_context);
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()				
				.UseSqlite("DataSource:=memory")
				.Options;
		}
		
		[Fact]
		public async Task AddCategory_AddCategoryAsync_ShouldAddToList()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			//Act
			await _categoryRepo.AddCategoryAsync(newCategory);
			//Assert
			var result = _context.Categories.Find(newCategory.Id);
			Assert.NotNull(result);
			Assert.Equal(newCategory.Name, result.Name);
		}		
		[Fact]
		public async Task DeleteCategory_DeleteCategoryAsync_ShouldRemoveFromList()
		{
			//Arrange
			var idCategory = 20;
			//Act
			await _categoryRepo.DeleteCategoryAsync(idCategory);
			//Assert
			var result = _context.Categories.Find(idCategory);
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
			_context.Categories.Add(newCategory);
			_context.SaveChanges();
			var idCategory = 1;
			//Act
			await _categoryRepo.SwitchOffCategoryAsync(idCategory);
			//Assert
			var result = _context.Categories.Find(idCategory);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
	}
}
