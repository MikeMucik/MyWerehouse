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
		Task AddCategoryAsync(CategoryDTO categoryDTO);		
		Task DeleteCategoryAsync(int id);
		Task UpdateCategoryAsync(CategoryDTO categoryDTO);
		Task<ListCategoriesDTO> GetCategoriesAsync(int pageSize, int pageNumber);
	}
}
