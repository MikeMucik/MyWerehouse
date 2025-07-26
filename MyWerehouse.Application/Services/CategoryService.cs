using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ICategoryRepo _categoryRepo;
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		private readonly IValidator<CategoryDTO> _validator;
		private readonly WerehouseDbContext _werehouseDbContext;
		public CategoryService(
			ICategoryRepo categoryRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IProductRepo? productRepo = null,
			IValidator<CategoryDTO>? validator = null)
		{
			_categoryRepo = categoryRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_productRepo = productRepo;
			_validator = validator;
		}
		public CategoryService(
			ICategoryRepo categoryRepo,
			IMapper mapper
			)
		{
			_categoryRepo = categoryRepo;
			_mapper = mapper;
		}

		public async Task AddCategoryAsync(CategoryDTO categoryDTO)
		{
			if (string.IsNullOrEmpty(categoryDTO.Name))
			{
				throw new InvalidDataException("Brak nazwy kategorii");
			}
			if (await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name) != null)
			{
				throw new InvalidDataException("Kategoria o tej nazwie już istnieje.");
			}
			var category = _mapper.Map<Category>(categoryDTO);
			await _categoryRepo.AddCategoryAsync(category);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task DeleteCategoryAsync(int id)
		{
			if (await _categoryRepo.GetCategoryByIdAsync(id) != null)
			{
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
					await _categoryRepo.DeleteCategoryAsync(id);
				}
				await _werehouseDbContext.SaveChangesAsync();
			}
			else { throw new ArgumentException($"Kategoria o ID {id} nie została znalezniona", nameof(id)); }
		}
		public async Task UpdateCategoryAsync(CategoryDTO categoryDTO)
		{
			if (string.IsNullOrEmpty(categoryDTO.Name))
			{
				throw new InvalidDataException("Brak nazwy kategorii - proszę podać");
			}
			var existingCategory = await _categoryRepo.GetCategoryByIdAsync(categoryDTO.Id);
			if (existingCategory != null)
			{
				var categoryWithSameName = await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name);
				if (categoryWithSameName != null && categoryWithSameName.Id == existingCategory.Id)
				{
					throw new InvalidDataException("Kategoria o tej nazwie już istnieje.");
				}
				existingCategory.Name = categoryDTO.Name;
				await _werehouseDbContext.SaveChangesAsync();
			}
			else throw new ArgumentException($"Brak kategori o numerze {existingCategory.Id}");
		}
		public async Task<ListCategoriesDTO> GetCategoriesAsync(int pageSize, int pageNumber)
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
			return categoriesList;
		}
	}
}
