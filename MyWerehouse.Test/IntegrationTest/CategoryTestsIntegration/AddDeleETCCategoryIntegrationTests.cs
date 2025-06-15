using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.IntegrationTest.CategoryTestsIntegration
{
	public class AddDeleETCCategoryIntegrationTests : CategoryIntegrationCommand
	{
		[Fact]
		public void AddCategory_AddCategory_AddedToList()
		{
			//Arrange
			var category = new CategoryDTO
			{
				Name = "newCategory"
			};
			//Act
			_categoryService.AddCategory(category);
			//Assert
			var result = _context.Categories.FirstOrDefault(c => c.Name == category.Name);
			Assert.NotNull(result);
		}
		[Fact]
		public void AddCategoryNoName_AddCategory_NoAddedToList()
		{
			//Arrange
			var category = new CategoryDTO
			{
				Name = ""
			};
			//Act&Assert
			var quantity = _context.Categories.Count();
			var ex = Assert.Throws<InvalidDataException>(()=>
			_categoryService.AddCategory(category));
			var result = _context.Categories.Count();
			Assert.Equal(quantity, result);
			Assert.Contains("Brak nazwy kategorii", ex.Message);
		}
		[Fact]
		public void AddCategoryTheSameName_AddCategory_NoAddedToList()
		{
			//Arrange
			var category = new CategoryDTO
			{
				Name = "TestCategory"
			};
			//Act&Assert
			var quantity = _context.Categories.Count();
			var ex = Assert.Throws<InvalidDataException>(() =>
			_categoryService.AddCategory(category));
			var result = _context.Categories.Count();
			Assert.Equal(quantity, result);
			Assert.Contains("Kategoria", ex.Message);
		}
		[Fact]
		public void RemoveCategory_DeleteCategory_DeleteFromList()
		{
			//Arrange
			var categoryId = 3;
			//Act
			_categoryService.DeleteCategory(categoryId);
			//Assert
			var result = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
			Assert.Null(result);
		}
		[Fact]
		public void RemoveCategory_DeleteCategory_HideCategory()
		{
			//Arrange
			var categoryId = 1;
			//Act
			_categoryService.DeleteCategory(categoryId);
			//Assert
			var result = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
		[Fact]
		public void RemoveNotExistingCategory_DeleteCategory_ThrowException()
		{
			//Arrange
			var categoryId = 1000;
			//Act&Assert
			var ex = Assert.Throws<InvalidDataException>(()=>
			_categoryService.DeleteCategory(categoryId));
			Assert.Contains("Nie ma kategorii o tym numerze", ex.Message);
		}
		[Fact]
		public void UpdateCategoryName_UpdateCategory_ChangeName()
		{
			//Arrange			
			var updatingCategory = new Category { Id = 22, Name = "ToUpdateCategory" };
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Categories.Add(updatingCategory);
			arrangeContext.SaveChanges();
			//Act
			var updatedCategory = new CategoryDTO { Id = 22, Name = "NewTestCategory1" };
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _categoryRepo = new CategoryRepo(actContext);
				var _validator = new CategoryDTOValidation();
				var _categoryService = new CategoryService(_categoryRepo, _mapper, _validator);
				_categoryService.UpdateCategory(updatedCategory);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Categories.Find(updatingCategory.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedCategory.Name, result.Name);
			}
		}
		[Fact]
		public void UpdateCategoryNameNoName_UpdateCategory_ThrowException()
		{
			//Arrange			
			var updatingCategory = new Category { Id =44, Name = "ToUpdateCategory" };
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Categories.Add(updatingCategory);
			arrangeContext.SaveChanges();
			//Act&Assert
			var updatedCategory = new CategoryDTO { Id = 44, Name = "" };
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _categoryRepo = new CategoryRepo(actContext);
				var _validator = new CategoryDTOValidation();
				var _categoryService = new CategoryService(_categoryRepo, _mapper, _validator);
				var ex = Assert.Throws<InvalidDataException>(()=>
				_categoryService.UpdateCategory(updatedCategory));
				Assert.Contains("Brak nazwy kategorii - proszę podać", ex.Message);
			}			
		}

	}
}
