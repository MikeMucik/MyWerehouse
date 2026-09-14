using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Interfaces.Persistence;
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
		public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct)
		{
			if (id == Guid.Empty)
			{
				return null;
			}
			var product = await _werehouseDbContext.Products
				.FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false, ct);
			return product;
		}

		public async Task<string?> GetSKUForProductAsync(Guid id, CancellationToken ct)
		{
			return await _werehouseDbContext.Products
				.Where(p=>!p.IsDeleted && p.Id == id)
				.Select(p=>p.SKU)
				.FirstOrDefaultAsync(ct);
		}
		public async Task<Product?> GetProductToEditAsync(Guid id, CancellationToken ct)
		{
			if (id != Guid.Empty)
			{
				var product = await _werehouseDbContext.Products
					.Where(x => !x.IsDeleted)
					.Include(p => p.Details)
					.FirstOrDefaultAsync(p => p.Id == id, ct);
				return product;
			}
			return null;
		}
		public Task<bool> IsExistProduct(Guid id, CancellationToken ct)
		{
			return _werehouseDbContext.Products
				.Where(x=>!x.IsDeleted)
				.AnyAsync(p=>p.Id == id, ct);	
		}

		public Task<bool> HasProductsInCategory(int categoryId, CancellationToken ct)
		{
			return _werehouseDbContext.Products
					   .AnyAsync(p => p.CategoryId == categoryId, ct);
		}

		public Task<bool> AlreadyExist(string name, string sku)
		{
			return _werehouseDbContext.Products
				.AnyAsync(p => p.Name == name || p.SKU == sku);
		}
	}
}
