using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.CategoryTestsIntegration
{
	[Collection("QueryCollectionInMemory")]
	public class ViewCategoryIntegrationTests : CommandTestBase
	{
		private readonly CategoryService _categoryService;
		private readonly CategoryRepo _categoryRepo;
		private readonly IProductRepo _productRepo;
		private readonly IValidator<CategoryDTO> _validator;

		public ViewCategoryIntegrationTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_categoryRepo = new CategoryRepo(_context);
			_productRepo = new ProductRepo(_context);
			_validator = new CategoryDTOValidation();
			_categoryService = new CategoryService(_categoryRepo, _mapper, _context, _productRepo, _validator);
		}
		[Fact]
		public async Task GetCategoriesAsync_ShouldReturnCategories_WhenDataExist()
		{
			//Arrange
			var pageSize = 5;
			var pagenumber = 1;
			var ct = CancellationToken.None;
			//Act
			var result = await _categoryService.GetCategoriesAsync(pagenumber, pageSize, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(3, result.Result.Items.Count);
		}
	}
}
