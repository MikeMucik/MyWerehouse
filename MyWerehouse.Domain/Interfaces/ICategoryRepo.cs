using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface ICategoryRepo
	{
		void AddCategory(Category category);
		Task AddCategoryAsync(Category category);		
		void DeleteCategory(int idCategory);
		Task DeleteCategoryAsync(int idCategory);
		void SwitchOffCategory(int idCategory);
		Task SwitchOffCategoryAsync(int idCategory);				
		Category? GetCategoryById(int id);
		Task<Category?> GetCategoryByIdAsync(int id);
		Category? GetCategoryByName(string name);
		Task<Category?> GetCategoryByNameAsync(string name);
		IQueryable<Category> GetAllCategories();		
	}
}
