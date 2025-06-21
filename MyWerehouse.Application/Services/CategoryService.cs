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

namespace MyWerehouse.Application.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ICategoryRepo _categoryRepo;
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		private readonly IValidator<CategoryDTO> _validator;
		public CategoryService(
			ICategoryRepo categoryRepo,			
			IMapper mapper,
			IProductRepo? productRepo = null,
			IValidator<CategoryDTO>? validator = null)
		{
			_categoryRepo = categoryRepo;			
			_mapper = mapper;
			_productRepo = productRepo;
			_validator = validator;
		}
		public CategoryService(
			ICategoryRepo categoryRepo,
			IMapper mapper
			, IValidator<CategoryDTO>? validator
			)
		{
			_categoryRepo = categoryRepo;
			_mapper = mapper;
			_validator = validator;
		}
		//public CategoryService(
		//	ICategoryRepo categoryRepo,
		//	IMapper mapper			
		//	)
		//{
		//	_categoryRepo = categoryRepo;
		//	_mapper = mapper;			
		//}
		public void AddCategory(CategoryDTO categoryDTO)
		{
			if (string.IsNullOrEmpty(categoryDTO.Name))
			{
				throw new InvalidDataException("Brak nazwy kategorii");
			}
			if (_categoryRepo.GetCategoryByName(categoryDTO.Name) == null)
			{
				var category = _mapper.Map<Category>(categoryDTO);
				_categoryRepo.AddCategory(category);
			}
			else { throw new InvalidDataException("Kategoria o tej nazwie już istnieje."); }
		}
		public async Task AddCategoryAsync(CategoryDTO categoryDTO)
		{
			if (string.IsNullOrEmpty(categoryDTO.Name))
			{
				throw new InvalidDataException("Brak nazwy kategorii");
			}
			if (await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name) == null)
			{
				var category = _mapper.Map<Category>(categoryDTO);
				await _categoryRepo.AddCategoryAsync(category);
			}
			else { throw new InvalidDataException("Kategoria o tej nazwie już istnieje."); }
		}
		public void DeleteCategory(int id)
		{
			if (_categoryRepo.GetCategoryById(id) != null)
			{
				var filter = new ProductSearchFilter//do przemyślenia
				{
					CategoryId = id,					
				};
				var products = _productRepo.FindProducts(filter);
				if (products.Any())
				{
					_categoryRepo.SwitchOffCategory(id);
				}
				else
				{
					_categoryRepo.DeleteCategory(id);
				}
			}
			else { throw new ArgumentException($"Kategoria o ID {id} nie została znalezniona", nameof(id)); }
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
			}
			else { throw new ArgumentException($"Kategoria o ID {id} nie została znalezniona", nameof(id)); }
		}
		
		public void UpdateCategory(CategoryDTO categoryDTO)
		{
			if (string.IsNullOrEmpty(categoryDTO.Name))
			{
				throw new InvalidDataException("Brak nazwy kategorii - proszę podać");
			}
			if (_categoryRepo.GetCategoryByName(categoryDTO.Name) == null)
			{
				var category = _mapper.Map<Category>(categoryDTO);
				_categoryRepo.UpdateCategory(category);
			}			
			else { throw new InvalidDataException("Kategoria o tej nazwie już istnieje."); }			
		}
		public async Task UpdateCategoryAsync(CategoryDTO categoryDTO)
		{
			if (string.IsNullOrEmpty(categoryDTO.Name))
			{
				throw new InvalidDataException("Brak nazwy kategorii - proszę podać");
			}
			if (await _categoryRepo.GetCategoryByNameAsync(categoryDTO.Name) == null)
			{
				var category = _mapper.Map<Category>(categoryDTO);
				await _categoryRepo.UpdateCategoryAsync(category);
			}
			else { throw new InvalidDataException("Kategoria o tej nazwie już istnieje."); }
		}
		public ListCategoriesDTO GetCategories(int pageSize, int pageNumber)
		{
			var categories = _categoryRepo.GetAllCategories()
				.OrderBy(c => c.Name)
				.ProjectTo<CategoryDTO>(_mapper.ConfigurationProvider);
			var categoriesToShow = categories
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToList();
			var categoriesList = new ListCategoriesDTO()
			{
				Categories = categoriesToShow,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count = categories.Count()
			};
			return categoriesList;
		}
		public async Task<ListCategoriesDTO> GetCategoriesAsync(int pageSize, int pageNumber)
		{
			var categories = _categoryRepo.GetAllCategories()
				.OrderBy(c => c.Name)
				.ProjectTo<CategoryDTO>(_mapper.ConfigurationProvider);
			var categoriesToShow =await categories
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync();
			var categoriesList = new ListCategoriesDTO()
			{
				Categories = categoriesToShow,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count =await categories.CountAsync()
			};
			return categoriesList;
		}

	}
}
