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
		Task AddCategoryAsync(Category category);				
		Task DeleteCategoryAsync(int idCategory);		
		Task SwitchOffCategoryAsync(int idCategory);	
		Task<Category?> GetCategoryByIdAsync(int id);		
		Task<Category?> GetCategoryByNameAsync(string name);
		IQueryable<Category> GetAllCategories();		
	}
}
