using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.CategoryModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface ICategoryService
	{
		void AddCategory(CategoryDTO categoryDTO);
		Task AddCategoryAsync(CategoryDTO categoryDTO);
		void DeleteCategory(int id);
		Task DeleteCategoryAsync(int id);
		void UpdateCategory(CategoryDTO categoryDTO);
		Task UpdateCategoryAsync(CategoryDTO categoryDTO);
		ListCategoriesDTO GetCategories(int pageSize, int pageNumber);
		Task<ListCategoriesDTO> GetCategoriesAsync(int pageSize, int pageNumber);
	}
}
