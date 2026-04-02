using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class ProductRepo : IProductRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public ProductRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public Product AddProduct(Product product)
		{
			_werehouseDbContext.Products.Add(product);
			return product;
		}
		public void DeleteProduct(Product product)
		{
			_werehouseDbContext.Remove(product);
		}
		public void SwitchOffProduct(Product product)
		{
			//product.IsDeleted = true;
		}
		public async Task<Product?> GetProductByIdAsync(Guid id)
		{
			if (id != Guid.Empty || id != null)//
			{
				var product = await _werehouseDbContext.Products
					.FirstOrDefaultAsync(p => p.Id == id);
				return product;
			}
			return null;
		}
		public async Task<Product?> GetProductToEditAsync(Guid id)
		{
			if (id != Guid.Empty)
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
				.Where(p => p.IsDeleted == false);
			//if (filter.ProductId > 0)
			//{
			//	result = result.Where(p => p.ProductId == filter.ProductId);
			//}
			if (!string.IsNullOrEmpty(filter.ProductName))
			{
				result = result.Where(p => p.Name != null && p.Name.StartsWith(filter.ProductName));

			}
			if (!string.IsNullOrEmpty(filter.SKU))
			{
				result = result.Where(p => p.SKU != null && p.SKU.StartsWith(filter.SKU));
			}
			if (!string.IsNullOrEmpty(filter.Category))
			{
				result = result.Where(p => p.Category.Name != null && p.Category.Name.StartsWith(filter.Category));
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

		public async Task<bool> IsExistProduct(Guid id)
		{
			if (await _werehouseDbContext.Products.FindAsync(id) != null) { return true; }
			return false;
		}

		//public async Task<bool> EnsureAllExist(List<int> ids)
		//{
		//	foreach (var id in ids)
		//	{
		//		if (await _werehouseDbContext.Products.FindAsync(id) == null)
		//		{ return true; }
		//	}
		//	return false;
		//}
	}
}