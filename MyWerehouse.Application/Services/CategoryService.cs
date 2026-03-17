using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ICategoryRepo _categoryRepo;
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		private readonly IValidator<CategoryDTO> _validator;//
		private readonly WerehouseDbContext _werehouseDbContext;
		public CategoryService(
			ICategoryRepo categoryRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IProductRepo? productRepo = null,
			IValidator<CategoryDTO>? validator = null)//
		{
			_categoryRepo = categoryRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_productRepo = productRepo;
			_validator = validator;//
		}
		public CategoryService(
			ICategoryRepo categoryRepo,
			IMapper mapper
			)
		{
			_categoryRepo = categoryRepo;
			_mapper = mapper;
		}

		public async Task<AppResult<Unit>> AddCategoryAsync(CategoryDTO categoryDTO)
		{
			var validationResult = await _validator.ValidateAsync(categoryDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			//if (string.IsNullOrEmpty(categoryDTO.Name))
			//{
			//	throw new NotFoundCategoryException("Brak nazwy kategorii.");
			//}
			if (await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name) != null)
			{
				return AppResult<Unit>.Fail("Kategoria o tej nazwie już istnieje.", ErrorType.Conflict);				
			}
			var category = _mapper.Map<Category>(categoryDTO);
			_categoryRepo.AddCategory(category);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value, "Dodano kategorię.");
		}
		public async Task<AppResult<Unit>> DeleteCategoryAsync(int id)
		{
			var category = await _categoryRepo.GetCategoryByIdAsync(id);
			if (category == null) return AppResult<Unit>.Fail($"Kategoria o ID {id} nie została znalezniona", ErrorType.NotFound);
				var filter = new ProductSearchFilter
				{
					CategoryId = id,
				};
				var products = _productRepo.FindProducts(filter);
				if (await products.AnyAsync())
				{
					await _categoryRepo.SwitchOffCategoryAsync(id);
				}
				else
				{
					 _categoryRepo.DeleteCategory(category);
				}
				await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value, "Kategoria została usunięta.");
		}
		public async Task<AppResult<Unit>> UpdateCategoryAsync(CategoryDTO categoryDTO)
		{
			var validationResult = await _validator.ValidateAsync(categoryDTO);
			if (validationResult != null)
				if (!validationResult.IsValid)
				{
					throw new ValidationException(validationResult.Errors);
				}
			//if (string.IsNullOrEmpty(categoryDTO.Name))
			//{
			//	throw new NotFoundCategoryException("Brak nazwy kategorii - proszę podać");
			//}
			var existingCategory = await _categoryRepo.GetCategoryByIdAsync(categoryDTO.Id);
			if (existingCategory != null)
			{
				var categoryWithSameName = await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name);
				if (categoryWithSameName != null && categoryWithSameName.Id == existingCategory.Id)
				{
					return AppResult<Unit>.Fail("Kategoria o tej nazwie już istnieje.", ErrorType.Conflict);
				}
				existingCategory.Name = categoryDTO.Name;
				await _werehouseDbContext.SaveChangesAsync();
				return AppResult<Unit>.Success(Unit.Value, "Kategorię zaktualizowano.");
			}
			else return AppResult<Unit>.Fail($"Brak kategori o numerze {existingCategory.Id}", ErrorType.NotFound);
		}
		public async Task<AppResult<ListCategoriesDTO>> GetCategoriesAsync(int pageSize, int pageNumber)
		{
			var categories = _categoryRepo.GetAllCategories()
				.OrderBy(c => c.Name)
				.ProjectTo<CategoryDTO>(_mapper.ConfigurationProvider);
			var categoriesToShow = await categories
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync();
			var categoriesList = new ListCategoriesDTO()
			{
				Categories = categoriesToShow,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count = await categories.CountAsync()
			};
			return AppResult<ListCategoriesDTO>.Success(categoriesList);
		}

		public async Task<AppResult<CategoryDTO>> GetCategoryByIdAsync(int id)
		{
			var result = _categoryRepo.GetCategoryByIdAsync(id);
			if (result == null)
			{
				return AppResult<CategoryDTO>.Fail("Nie znaleziono kategorii.");
			}
			var categoryDTO = _mapper.Map<CategoryDTO>(result);
			return AppResult<CategoryDTO>.Success(categoryDTO);
		}
	}
}
