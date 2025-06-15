using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.CategoryTestsIntegration
{
	public class CategoryIntegrationCommand :CommandTestBase
	{
		public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly CategoryService _categoryService;
		public readonly IMapper _mapper;
		public readonly IValidator<CategoryDTO> _validator;
		public CategoryIntegrationCommand() : base()
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("SharedDatabase")
				.Options;
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();
			var _categoryRepo = new CategoryRepo(_context);
			var _productRepo = new ProductRepo(_context);
			_validator = new CategoryDTOValidation();
			_categoryService = new CategoryService(_categoryRepo,_mapper, _productRepo,  _validator);
		}

	}
}
