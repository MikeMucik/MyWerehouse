using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class CategoryRepo : ICategoryRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public CategoryRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
				
		public async Task AddCategoryAsync(Category category)
		{
			await _werehouseDbContext.Categories.AddAsync(category);			
		}		
		public async Task DeleteCategoryAsync(int idCategory)
		{
			var category =await _werehouseDbContext.Categories.FindAsync(idCategory);
			if (category != null)
			{
				_werehouseDbContext.Categories.Remove(category);			
			}
		}		
		public async Task SwitchOffCategoryAsync(int idCategory)
		{
			var category = await _werehouseDbContext.Categories.FindAsync(idCategory);
			if (category != null)
			{
				category.IsDeleted = true;
				await _werehouseDbContext.SaveChangesAsync();
			}
		}				
		public async Task<Category?> GetCategoryByIdAsync(int id)
		{
			return await _werehouseDbContext.Categories.SingleOrDefaultAsync(c => c.Id == id);
		}		
		public async Task<Category?> GetCategoryByNameAsync(string name)
		{
			return await _werehouseDbContext.Categories.SingleOrDefaultAsync(c => c.Name == name);
		}
		public IQueryable<Category> GetAllCategories()
		{
			return _werehouseDbContext.Categories;
		}		
	}
}
