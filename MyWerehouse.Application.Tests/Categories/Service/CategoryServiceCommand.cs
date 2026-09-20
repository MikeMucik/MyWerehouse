using Moq;
using MyWerehouse.Application.Categories.Services;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.ViewModels.CategoryModels;

namespace MyWerehouse.Application.Tests.Categories.Service
{
	public class CategoryServiceCommand
	{
		protected readonly CategoryService _categoryService;
		protected readonly Mock<ICategoryRepo> _categoryRepo = new();
		protected readonly Mock<IUnitOfWork> _unitOfWork = new();
		protected readonly Mock<IProductRepo> _productRepo = new();

		public CategoryServiceCommand()
		{
			var validator = new CategoryDTOValidation();
			var categoryReadService = new Mock<ICategoryReadService>();
			_categoryService = new CategoryService(_unitOfWork.Object, _categoryRepo.Object,
				_productRepo.Object, validator, categoryReadService.Object);
		}
	}
}
