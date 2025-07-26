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

namespace MyWerehouse.Test.IntegrationTestService.CategoryTestsIntegration
{
	public class AddDeleteUpdateCategoryIntegrationTests : CategoryIntegrationCommand
	{		
		[Fact]
		public async Task AddCategory_AddCategoryAsync_AddedToList()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "TestCategory"

			};
			var product = new Product
			{
				Id = 1,
				Name = "fdsfd",
				CategoryId = 1,
				SKU = "aaa",
				IsDeleted = false,
				CartonsPerPallet = 56
			};

			_context.Products.Add(product);
			_context.Categories.Add(category);
			_context.SaveChanges();
			var categoryDTO = new CategoryDTO
			{
				Name = "newCategory"
			};
			//Act
			await _categoryService.AddCategoryAsync(categoryDTO);
			//Assert
			var result = _context.Categories.FirstOrDefault(c => c.Name == category.Name);
			Assert.NotNull(result);
		}
		[Fact]
		public async Task AddCategoryNoName_AddCategoryAsync_NoAddedToList()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "TestCategory"

			};
			var product = new Product
			{
				Id = 1,
				Name = "fdsfd",
				CategoryId = 1,
				SKU = "aaa",
				IsDeleted = false,
				CartonsPerPallet = 56
			};

			_context.Products.Add(product);
			_context.Categories.Add(category);
			_context.SaveChanges();
			var categoryDTO = new CategoryDTO
			{
				Name = ""
			};
			//Act&Assert
			var quantity = _context.Categories.Count();
			var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
			_categoryService.AddCategoryAsync(categoryDTO));
			var result = _context.Categories.Count();
			Assert.Equal(quantity, result);
			Assert.Contains("Brak nazwy kategorii", ex.Message);
		}
		[Fact]
		public async Task AddCategoryTheSameName_AddCategoryAsync_NoAddedToList()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "TestCategory"

			};
			var product = new Product
			{
				Id = 1,
				Name = "fdsfd",
				CategoryId = 1,
				SKU = "aaa",
				IsDeleted = false,
				CartonsPerPallet = 56
			};

			_context.Products.Add(product);
			_context.Categories.Add(category);
			_context.SaveChanges();
			var categoryDTO = new CategoryDTO
			{
				Name = "TestCategory"
			};
			//Act&Assert
			var quantity = _context.Categories.Count();
			var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
			_categoryService.AddCategoryAsync(categoryDTO));
			var result = _context.Categories.Count();
			Assert.Equal(quantity, result);
			Assert.Contains("Kategoria", ex.Message);
		}
		[Fact]
		public async Task RemoveCategory_DeleteCategoryAsync_DeleteFromList()
		{
			//Arrange
			var category = new Category
			{
				Id = 3,
				Name = "TestCategory"

			};
			_context.Categories.Add(category);
			_context.SaveChanges();
			var categoryId = 3;
			//Act
			await _categoryService.DeleteCategoryAsync(categoryId);
			//Assert
			var result = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
			Assert.Null(result);
		}
		[Fact]
		public async Task RemoveCategory_DeleteCategoryAsync_HideCategory()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "TestCategory"

			};
			var product = new Product
			{
				Id = 1,
				Name = "fdsfd",
				CategoryId = 1,
				SKU = "aaa",
				IsDeleted = false,
				CartonsPerPallet = 56
			};

			_context.Products.Add(product);
			_context.Categories.Add(category);
			_context.SaveChanges();
			var categoryId = 1;
			//Act
			await _categoryService.DeleteCategoryAsync(categoryId);
			//Assert
			var result = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}

		[Fact]
		public async Task UpdateCategoryName_UpdateCategoryAsync_ChangeName()
		{
			//Arrange			
			var updatingCategory = new Category { Id = 66, Name = "ToUpdateCategoryAsync" };
			_context.Categories.Add(updatingCategory);
			_context.SaveChanges();
			//Act
			var updatedCategory = new CategoryDTO { Id = 66, Name = "NewTestCategoryAsync1" };
			await _categoryService.UpdateCategoryAsync(updatedCategory);
			//Assert
			var result = _context.Categories.Find(updatingCategory.Id);
			Assert.NotNull(result);
			Assert.Equal(updatedCategory.Name, result.Name);
		}
		[Fact]
		public async Task UpdateCategoryNameNoName_UpdateCategoryAsync_ThrowException()
		{
			//Arrange			
			var updatingCategory = new Category { Id = 88, Name = "ToUpdateCategory" };			
			_context.Categories.Add(updatingCategory);
			_context.SaveChanges();
			//Act&Assert
			var updatedCategory = new CategoryDTO { Id = 88, Name = "" };
			var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
				_categoryService.UpdateCategoryAsync(updatedCategory));
				Assert.Contains("Brak nazwy kategorii - proszę podać", ex.Message);			
		}
	}
}
