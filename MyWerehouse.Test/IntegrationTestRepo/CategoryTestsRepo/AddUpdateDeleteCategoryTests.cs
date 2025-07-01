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
				.UseInMemoryDatabase("TestDatabase")
				.Options;
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
			//Assert
			var result = _context.Categories.Find(newCategory.Id);
			Assert.NotNull(result);
			Assert.Equal(newCategory.Name, result.Name);
		}
		//[Fact]
		//public async Task AddCategory_AddCategoryAsync_ShouldAddToList()
		//{
		//	//Arrange
		//	var newCategory = new Category
		//	{				
		//		Name = "CategoryName"
		//	};
		//	//Act
		//	await _categoryRepo.AddCategoryAsync(newCategory);
		//	//Assert
		//	var result = _context.Categories.Find(newCategory.Id);
		//	Assert.NotNull(result);
		//	Assert.Equal(newCategory.Name, result.Name);
		//}
		//[Fact]
		//public void DeleteCategory_DeleteCategory_ShouldRemoveFromList()
		//{
		//	//Arrange
		//	var idCategory = 20;
		//	//Act
		//	_categoryRepo.DeleteCategory(idCategory);
		//	//Assert
		//	var result = _context.Categories.Find(idCategory);
		//	Assert.Null(result);
		//}
		//[Fact]
		//public async Task DeleteCategory_DeleteCategoryAsync_ShouldRemoveFromList()
		//{
		//	//Arrange
		//	var idCategory = 20;
		//	//Act
		//	await _categoryRepo.DeleteCategoryAsync(idCategory);
		//	//Assert
		//	var result = _context.Categories.Find(idCategory);
		//	Assert.Null(result);
		//}
		//[Fact]
		//public void SwithOffCategory_SwithOffCategory_ShouldHideFromList()
		//{
		//	//Arrange
		//	var newCategory = new Category
		//	{
		//		Name = "CategoryName"
		//	};
		//	_context.Categories.Add(newCategory);
		//	_context.SaveChanges();
		//	var idCategory = 1;
		//	//Act
		//	_categoryRepo.SwitchOffCategory(idCategory);
		//	//Assert
		//	var result = _context.Categories.Find(idCategory);
		//	Assert.NotNull(result);
		//	Assert.True(result.IsDeleted);
		//}
		//[Fact]
		//public async Task SwithOffCategory_SwithOffCategoryAsync_ShouldHideFromList()
		//{
		//	//Arrange
		//	var newCategory = new Category
		//	{
		//		Name = "CategoryName"
		//	};
		//	_context.Categories.Add(newCategory);
		//	_context.SaveChanges();
		//	var idCategory = 1;
		//	//Act
		//	await _categoryRepo.SwitchOffCategoryAsync(idCategory);
		//	//Assert
		//	var result = _context.Categories.Find(idCategory);
		//	Assert.NotNull(result);
		//	Assert.True(result.IsDeleted);
		//}
		//[Fact]
		//public void UpdateCategoryName_UpdateCategory_ChangeName()
		//{
		//	//Arrange			
		//	var updatingCategory = new Category { Id = 122, Name = "ToUpdateCategory" };
		//	using var arrangeContext = new WerehouseDbContext(_contextOptions);
		//	arrangeContext.Categories.Add(updatingCategory);
		//	arrangeContext.SaveChanges();
		//	//Act
		//	var updatedCategory = new Category { Id = 122, Name = "NewCategoryName" };
		//	using (var actContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var repo = new CategoryRepo(actContext);
		//		repo.UpdateCategory(updatedCategory);
		//	}
		//	//Assert
		//	using (var assertContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var result = assertContext.Categories.Find(updatingCategory.Id);	
		//		Assert.NotNull(result);
		//		Assert.Equal(updatedCategory.Name, result.Name);
		//	}
		//}
		//[Fact]
		//public async Task UpdateCategoryName_UpdateCategoryAsync_ChangeName()
		//{
		//	//Arrange			
		//	var updatingCategory = new Category { Id = 22, Name = "ToUpdateCategory" };
		//	using var arrangeContext = new WerehouseDbContext(_contextOptions);
		//	arrangeContext.Categories.Add(updatingCategory);
		//	arrangeContext.SaveChanges();
		//	//Act
		//	var updatedCategory = new Category { Id = 22, Name = "NewCategoryName" };
		//	using (var actContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var repo = new CategoryRepo(actContext);
		//		await repo.UpdateCategoryAsync(updatedCategory);
		//	}
		//	//Assert
		//	using (var assertContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var result = assertContext.Categories.Find(updatingCategory.Id);
		//		Assert.NotNull(result);
		//		Assert.Equal(updatedCategory.Name, result.Name);
		//	}
		//}
	}
}
