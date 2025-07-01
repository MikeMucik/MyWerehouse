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

		public void AddCategory(Category category)
		{
			_werehouseDbContext.Categories.Add(category);
			//_werehouseDbContext.SaveChanges();
		}
		public async Task AddCategoryAsync(Category category)
		{
			await _werehouseDbContext.Categories.AddAsync(category);
			//await _werehouseDbContext.SaveChangesAsync();
		}
		public void DeleteCategory(int idCategory)
		{
			var category = _werehouseDbContext.Categories.Find(idCategory);
			if (category != null)
			{
				_werehouseDbContext.Categories.Remove(category);
				//_werehouseDbContext.SaveChanges();
			}
		}
		public async Task DeleteCategoryAsync(int idCategory)
		{
			var category =await _werehouseDbContext.Categories.FindAsync(idCategory);
			if (category != null)
			{
				_werehouseDbContext.Categories.Remove(category);
			//	await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void SwitchOffCategory(int idCategory)
		{
			var category = _werehouseDbContext.Categories.Find(idCategory);
			if (category != null)
			{
				category.IsDeleted = true;
				_werehouseDbContext.SaveChanges();
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
		//public void UpdateCategory(Category category)
		//{
		//	//_werehouseDbContext.Attach(category);			
		//	//_werehouseDbContext.Entry(category).Property(nameof(category.Name)).IsModified = true;
		//	_werehouseDbContext.SaveChanges();
			
		//}
		//public async Task UpdateCategoryAsync(Category category)
		//{
		//	//_werehouseDbContext.Attach(category);			
		//	//_werehouseDbContext.Entry(category).Property(nameof(category.Name)).IsModified = true;
		//	await _werehouseDbContext.SaveChangesAsync();			
		//}
		public Category? GetCategoryById(int id)
		{
			return _werehouseDbContext.Categories.SingleOrDefault(c=>c.Id ==id);
		}
		public async Task<Category?> GetCategoryByIdAsync(int id)
		{
			return await _werehouseDbContext.Categories.SingleOrDefaultAsync(c => c.Id == id);
		}
		public	Category? GetCategoryByName(string name)
		{
			return _werehouseDbContext.Categories.SingleOrDefault(c => c.Name == name);
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
