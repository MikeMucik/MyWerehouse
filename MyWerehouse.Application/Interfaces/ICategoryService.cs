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
		void DeleteCategory(int  id);
		void UpdateCategory(CategoryDTO categoryDTO);
		ListCategoriesDTO GetCategories(int pageSize, int pageNumber);
	}
}
