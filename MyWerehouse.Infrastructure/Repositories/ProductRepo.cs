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
		public Product GetProductById(int id)
		{
			if (id > 0)
			{
				var product = _werehouseDbContext.Products
					.Include(p => p.Category)
					.Include(p => p.Details)
					.FirstOrDefault(p => p.Id == id);
				return product;
			}
			return null;
		}
		public void UpdateProduct(Product product)
		{
			_werehouseDbContext.Attach(product);
			if (product.Name != null)
			{
				_werehouseDbContext.Entry(product).Property(nameof(product.Name)).IsModified = true;
			}
			if (product.SKU != null)
			{
				_werehouseDbContext.Entry(product).Property(nameof(product.SKU)).IsModified = true;
			}
			if (product.Category != null)
			{
				_werehouseDbContext.Entry(product).Property(nameof(product.Category)).IsModified = true;
			}
			if (product.CategoryId != 0)
			{
				_werehouseDbContext.Entry(product).Property(nameof(product.CategoryId)).IsModified = true;
			}
			if (product.Details != null)
			{
				_werehouseDbContext.Entry(product.Details).State = EntityState.Modified;
			}
			_werehouseDbContext.SaveChanges();

			//if (_werehouseDbContext.Products.FirstOrDefault(x => x.Id == product.Id) == null)
			//{
			//	return false;
			//}
			//if (product != null && _werehouseDbContext.Products.Find(product.Id) != null)
			//{
			//	_werehouseDbContext.Attach(product);
			//	if (product.Name != null)
			//	{
			//		_werehouseDbContext.Entry(product).Property(nameof(product.Name)).IsModified = true;
			//	}
			//	if (product.SKU != null)
			//	{
			//		_werehouseDbContext.Entry(product).Property(nameof(product.SKU)).IsModified = true;
			//	}
			//	if (product.CategoryId != 0)
			//	{
			//		_werehouseDbContext.Entry(product).Property(nameof(product.CategoryId)).IsModified = true;
			//	}
			//	_werehouseDbContext.SaveChanges();
			//	return true;
			//}
			//return false;
			//if (product == null)
			//{
			//	return false;
			//}
			//var existingProduct = _werehouseDbContext.Products.Find(product.Id);
			//if (existingProduct != null)
			//{
			//	if (product.Name != null)
			//	{
			//		existingProduct.Name = product.Name;
			//	}
			//	if (product.SKU != null)
			//	{
			//		existingProduct.SKU = product.SKU;
			//	}
			//	if (product.CategoryId != 0)
			//	{
			//		existingProduct.CategoryId = product.CategoryId;
			//	}
			//	if (product.Category != null)
			//	{
			//		existingProduct.Category = product.Category;
			//	}
			//	_werehouseDbContext.SaveChanges();
			//	return true;
			//}
			//return false;
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
		public IQueryable<Product> GetAllProducts()
		{
			return _werehouseDbContext.Products.Where(p=>p.IsDeleted ==false);
		}

		public IQueryable<Product> FindProducts(ProductSearchFilter filter)
		{
			var result = _werehouseDbContext.Products
				.Where(p => p.IsDeleted == false)
				.Include(p => p.Details)
				.Include(p => p.Category)
				.AsQueryable();
			//if (!string.IsNullOrEmpty(filter.ProductName))
			//{
			//	result = result.Where(p => p.Name == filter.ProductName);
			//}
			if (!string.IsNullOrEmpty(filter.ProductName))
			{
				result = result.Where(p => p.Name != null && p.Name.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase));
			}
			//if (!string.IsNullOrEmpty(filter.SKU))
			//{
			//	result = result.Where(p => p.SKU == filter.SKU);
			//}
			if (!string.IsNullOrEmpty(filter.SKU))
			{
				result = result.Where(p => p.SKU != null && p.SKU.Contains(filter.SKU, StringComparison.OrdinalIgnoreCase));
			}
			//if (!string.IsNullOrEmpty(filter.Category))
			//{
			//	result = result.Where(p => p.Category.Name == filter.Category);
			//}
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

		public void SwitchOffProduct(int id)
		{
			var product = _werehouseDbContext.Products.Find(id);
			if (product != null)
			{
				product.IsDeleted = true;
				_werehouseDbContext.SaveChanges();
			}
		}
	}
}