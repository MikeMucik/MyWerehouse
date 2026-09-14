using FluentValidation;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Services
{
	public class CategoryService : ICategoryService
	{
		private	readonly IUnitOfWork _unitOfWork;
		private readonly ICategoryRepo _categoryRepo;
		private readonly IProductRepo _productRepo;
		private readonly IValidator<CategoryDTO> _validator;
		private readonly ICategoryReadService _categoryReadService;
		public CategoryService(
			IUnitOfWork unitOfWork,
			ICategoryRepo categoryRepo,
			IProductRepo productRepo,
			IValidator<CategoryDTO> validator,
			ICategoryReadService categoryReadService)
		{
			_unitOfWork = unitOfWork;
			_categoryRepo = categoryRepo;
			_productRepo = productRepo;
			_validator = validator;
			_categoryReadService = categoryReadService;
		}

		public async Task<AppResult<Unit>> AddCategoryAsync(CategoryDTO categoryDTO, CancellationToken ct)
		{
			var validationResult = await _validator.ValidateAsync(categoryDTO, ct);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			if (await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name, ct) != null)
			{
				return AppResult<Unit>.Fail("A category with this name already exists.", ErrorType.Conflict);
			}
			var category = new Category
			{
				Name = categoryDTO.Name
			};
			_categoryRepo.AddCategory(category);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Category added.");
		}

		public async Task<AppResult<Unit>> DeleteCategoryAsync(int id, CancellationToken ct)
		{
			var category = await _categoryRepo.GetCategoryByIdAsync(id, ct);
			if (category == null) return AppResult<Unit>.Fail($"Category {id} was not found.");
			if (await _productRepo.HasProductsInCategory(id, ct))
			{
				await _categoryRepo.SwitchOffCategoryAsync(id, ct);
				await _unitOfWork.SaveChangesAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Category disabled.");
			}
			else
			{
				_categoryRepo.DeleteCategory(category);
				await _unitOfWork.SaveChangesAsync(ct); 
				return AppResult<Unit>.Success(Unit.Value, "Category deleted.");
			}
		}
		public async Task<AppResult<Unit>> UpdateCategoryAsync(int id, CategoryDTO categoryDTO, CancellationToken ct)
		{
			var validationResult = await _validator.ValidateAsync(categoryDTO, ct);
			if (validationResult != null)
				if (!validationResult.IsValid)
				{
					throw new ValidationException(validationResult.Errors);
				}
			var existingCategory = await _categoryRepo.GetCategoryByIdAsync(id, ct);
			if (existingCategory != null)
			{
				var categoryWithSameName = await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name, ct);
				if (categoryWithSameName != null && categoryWithSameName.Id != existingCategory.Id)
				{
					return AppResult<Unit>.Fail("A category with this name already exists.", ErrorType.Conflict);
				}
				existingCategory.Name = categoryDTO.Name;
				await _unitOfWork.SaveChangesAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Category updated.");
			}
			else return AppResult<Unit>.Fail($"Category {id} was not found.");
		}

		public async Task<AppResult<PagedResult<CategoryViewDTO>>> GetCategoriesAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var result = await _categoryReadService.GetCategoriesAsync(pageNumber, pageSize, ct);
			
			return AppResult<PagedResult<CategoryViewDTO>>.Success(result);
		}

		public async Task<AppResult<CategoryViewDTO>> GetCategoryByIdAsync(int id, CancellationToken ct)
		{
			var result = await _categoryRepo.GetCategoryByIdAsync(id, ct);
			if (result == null)
			{
				return AppResult<CategoryViewDTO>.Fail("Category was not found.");
			}
			var categoryDTO = new CategoryViewDTO
			{
				Name = result.Name,
			};
			return AppResult<CategoryViewDTO>.Success(categoryDTO);
		}
	}
}
