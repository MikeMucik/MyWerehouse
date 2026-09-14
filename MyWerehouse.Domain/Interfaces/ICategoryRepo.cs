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
		Task SwitchOffCategoryAsync(int idCategory, CancellationToken ct);
		Task<Category?> GetCategoryByIdAsync(int id, CancellationToken ct);
		Task<Category?> GetCategoryByNameAsync(string name, CancellationToken ct);
		
	}
}
