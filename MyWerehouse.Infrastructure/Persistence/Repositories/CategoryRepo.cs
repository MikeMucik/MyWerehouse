using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class CategoryRepo : ICategoryRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public CategoryRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void AddCategory(Category category)
		{
			 _werehouseDbContext.Categories.Add(category);
		}
		public void DeleteCategory(Category category)
		{
				_werehouseDbContext.Categories.Remove(category);
		}
		public async Task SwitchOffCategoryAsync(int idCategory, CancellationToken ct)
		{
			var category = await _werehouseDbContext.Categories.FindAsync([idCategory], ct);
			if (category != null)
			{
				category.IsDeleted = true;
			}
		}
		public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken ct)
		{
			return await _werehouseDbContext.Categories.SingleOrDefaultAsync(c => c.Id == id, ct);
		}
		public async Task<Category?> GetCategoryByNameAsync(string name, CancellationToken ct)
		{
			return await _werehouseDbContext.Categories.SingleOrDefaultAsync(c => c.Name == name, ct);
		}
		public IQueryable<Category> GetAllCategories()
		{
			return _werehouseDbContext.Categories;
		}
	}
}
