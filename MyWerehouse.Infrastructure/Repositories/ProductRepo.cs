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
			if (product.Name == null)
			{
				return -1;
			}
			else if (product.SKU == null)
			{
				return -2;
			}
			else if (product.CategoryId == 0)
			{
				return -3;
			}
			else
			{
				_werehouseDbContext.Products.Add(product);
				_werehouseDbContext.SaveChanges();
				return product.Id;
			}
		}


		public Product GetProductById(int id)
		{
			if (id > 0)
			{
				var product = _werehouseDbContext.Products
					.Include(p=>p.Category)
					.Include(p=>p.Details)
					.FirstOrDefault(p => p.Id == id);
				return product;
			}
			return null;
		}

		public bool UpdateProduct(Product product)
		{
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
			if (product == null)
			{
				return false;
			}
			var existingProduct = _werehouseDbContext.Products.Find(product.Id);
			if (existingProduct != null)
			{
				if (product.Name != null)
				{
					existingProduct.Name = product.Name;
				}
				if (product.SKU != null)
				{
					existingProduct.SKU = product.SKU;
				}
				if (product.CategoryId != 0)
					{ 
					existingProduct.CategoryId = product.CategoryId;
				}
				if (product.Category != null)
				{
					existingProduct.Category = product.Category;
				}
				_werehouseDbContext.SaveChanges();
				return true;
			}
			return false;
		}

		public bool DeleteProductById(int id)
		{
			var result = _werehouseDbContext.Products.FirstOrDefault(x => x.Id == id);
			if (result != null)
			{
				_werehouseDbContext.Remove(result);
				_werehouseDbContext.SaveChanges();
				return true;
			}
			return false;
		}

		public IQueryable<Product> GetAllProducts()
		{
			return _werehouseDbContext.Products;
		}

		public IQueryable< Product> FindProduct(string productName, string SKU , int Length, int Height, int Width, int Weight)
		{
			var result = _werehouseDbContext.Products
				.Include(p => p.Details)
				.AsQueryable();			
			if (productName != "")
			{
				result = result.Where(p => p.Name == productName);
			}
			if (SKU != "")
			{
				result = result.Where(p => p.SKU == SKU);
			}
			if (Height > 0)
			{
				result = result.Where(p => p.Details.Height == Height);
			}
			if (Weight > 0)
			{
				result = result.Where(p => p.Details.Weight == Weight);
			}
			if (Width > 0)
			{
				result = result.Where(p => p.Details.Width == Width);
			}
			if (Length> 0)
			{
				result = result.Where(p => p.Details.Length == Length);
			}

			return result;
		}
	}
}  