using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			_werehouseDbContext.SaveChanges();
		}

		public void DeleteCategory(int idCategory)
		{
			var category = _werehouseDbContext.Categories.Find(idCategory);
			if (category != null) { 
				_werehouseDbContext.Categories.Remove(category);
				_werehouseDbContext.SaveChanges();
			}
		}

		public IQueryable<Category> GetAllCategories()
		{
			return _werehouseDbContext.Categories;
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

		public void UpdateCategory(Category category)
		{
			_werehouseDbContext.Attach(category);
			if(category.Name != null)
			{
				_werehouseDbContext.Entry(category).Property(nameof(category.Name)).IsModified = true;
				_werehouseDbContext.SaveChanges();
			}
			
		}
	}
}
