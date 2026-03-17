using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.CategoryTestsIntegration
{
	public class CategoryIntegrationCommand : CommandTestBase
	{
		protected readonly CategoryService _categoryService;
		protected readonly IValidator<CategoryDTO> _validator;
		public CategoryIntegrationCommand() : base()
		{
			var _categoryRepo = new CategoryRepo(_context);
			var _productRepo = new ProductRepo(_context);
			_validator = new CategoryDTOValidation();
			_categoryService = new CategoryService(_categoryRepo,				
				_mapper, _context, _productRepo, 
				_validator
					 );
		}

	}
}
