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
		void UpdateCategory(Category category);
		void DeleteCategory(int idCategory);
		void SwitchOffCategory(int idCategory);
		IQueryable<Category> GetAllCategories();
	}
}
