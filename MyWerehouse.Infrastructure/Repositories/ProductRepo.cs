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
	public class ProductRepo : IProductRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public ProductRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public int AddProduct(Product product)
		{
			_werehouseDbContext.Products.Add(product);
			_werehouseDbContext.SaveChanges();
			return product.Id;
		}
		public async Task<int> AddProductAsync(Product product)
		{
			await _werehouseDbContext.Products.AddAsync(product);
			await _werehouseDbContext.SaveChangesAsync();
			return product.Id;
		}
		public void DeleteProductById(int id)
		{
			var result = _werehouseDbContext.Products.Find(id);
			if (result != null)
			{
				_werehouseDbContext.Remove(result);
				_werehouseDbContext.SaveChanges();
			}
		}
		public async Task DeleteProductByIdAsync(int id)
		{
			var result = await _werehouseDbContext.Products.FindAsync(id);
			if (result != null)
			{
				_werehouseDbContext.Remove(result);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void SwitchOffProduct(int id)
		{
			var product = _werehouseDbContext.Products.Find(id);
			if (product != null)
			{
				product.IsDeleted = true;
				_werehouseDbContext.SaveChangesAsync();
			}
		}
		public async Task SwitchOffProductAsync(int id)
		{
			var product = await _werehouseDbContext.Products.FindAsync(id);
			if (product != null)
			{
				product.IsDeleted = true;
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void UpdateProduct(Product product)
		{			
			_werehouseDbContext.SaveChanges();
		}
		public async Task UpdateProductAsync(Product product)
		{			
			await _werehouseDbContext.SaveChangesAsync();
		}
		public Product? GetProductById(int id)
		{
			if (id > 0)
			{
				var product = _werehouseDbContext.Products
					//.Include(p => p.Category)
					//.Include(p => p.Details)
					.FirstOrDefault(p => p.Id == id);
				return product;
			}
			return null;
		}
		public Product? GetProductToEdit(int id)
		{
			if (id > 0)
			{
				var product = _werehouseDbContext.Products					
					.Include(p => p.Details)
					.FirstOrDefault(p => p.Id == id);
				return product;
			}
			return null;
		}
		public async Task<Product?> GetProductByIdAsync(int id)
		{
			if (id > 0)
			{
				var product = await _werehouseDbContext.Products					
					.FirstOrDefaultAsync(p => p.Id == id);
				return product;
			}
			return null;
		}
		public async Task<Product?> GetProductToEditAsync(int id)
		{
			if (id > 0)
			{
				var product = await _werehouseDbContext.Products					
					.Include(p => p.Details)
					.FirstOrDefaultAsync(p => p.Id == id);
				return product;
			}
			return null;
		}
		public IQueryable<Product> GetAllProducts()
		{
			return _werehouseDbContext.Products.Where(p => p.IsDeleted == false);
		}		
		public IQueryable<Product> FindProducts(ProductSearchFilter filter)
		{
			var result = _werehouseDbContext.Products
				.Where(p => p.IsDeleted == false)
				.Include(p => p.Details)
				.Include(p => p.Category)
				.AsQueryable();
			if (!string.IsNullOrEmpty(filter.ProductName))
			{
				result = result.Where(p => p.Name != null && p.Name.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase));
			}
			if (!string.IsNullOrEmpty(filter.SKU))
			{
				result = result.Where(p => p.SKU != null && p.SKU.Contains(filter.SKU, StringComparison.OrdinalIgnoreCase));
			}
			if (!string.IsNullOrEmpty(filter.Category))
			{
				result = result.Where(p => p.Category.Name != null && p.Category.Name.Contains(filter.Category, StringComparison.OrdinalIgnoreCase));
			}
			if (filter.CategoryId > 0)
			{
				result = result.Where(p => p.CategoryId == filter.CategoryId);
			}
			if (filter.Height > 0)
			{
				result = result.Where(p => p.Details.Height == filter.Height);
			}
			if (filter.Weight > 0)
			{
				result = result.Where(p => p.Details.Weight == filter.Weight);
			}
			if (filter.Width > 0)
			{
				result = result.Where(p => p.Details.Width == filter.Width);
			}
			if (filter.Length > 0)
			{
				result = result.Where(p => p.Details.Length == filter.Length);
			}
			return result;
		}		
	}
}