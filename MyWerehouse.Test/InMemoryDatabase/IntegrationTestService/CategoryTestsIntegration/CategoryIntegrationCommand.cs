using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Infrastructure.Persistence.ReadServices;
using MyWerehouse.Test.InMemoryDatabase.Common;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.CategoryTestsIntegration
{
	public class CategoryIntegrationCommand : CommandTestBase
	{
		protected readonly CategoryService _categoryService;
		protected readonly IValidator<CategoryDTO> _validator;
		public CategoryIntegrationCommand() : base()
		{
			var _categoryRepo = new CategoryRepo(_context);
			var _productRepo = new ProductRepo(_context);
			var _unitOfWork = new UnitOfWork(_context);
			_validator = new CategoryDTOValidation();
			var _categoryReadServie = new CategoryReadService(_context);
			_categoryService = new CategoryService(_unitOfWork, _categoryRepo, _productRepo, _validator, _categoryReadServie);
		}
	}
}
