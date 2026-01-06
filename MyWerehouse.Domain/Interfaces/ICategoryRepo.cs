using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface ICategoryRepo
	{		
		void AddCategory(Category category);				
		void DeleteCategory(Category category);		
		Task SwitchOffCategoryAsync(int idCategory);	
		Task<Category?> GetCategoryByIdAsync(int id);		
		Task<Category?> GetCategoryByNameAsync(string name);
		IQueryable<Category> GetAllCategories();		
	}
}
