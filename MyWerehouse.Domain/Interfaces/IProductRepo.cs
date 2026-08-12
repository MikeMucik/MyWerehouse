using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Domain.Products.Models;


namespace MyWerehouse.Domain.Interfaces
{
	public interface IProductRepo
	{
		Product AddProduct(Product product);
		void DeleteProduct(Product product);
		Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct);
		Task<string?> GetSKUForProductAsync(Guid id, CancellationToken ct);
		Task<Product?> GetProductToEditAsync(Guid id, CancellationToken ct);
		Task<Product?> GetProductDetailsAsync(Guid id, CancellationToken ct);
		IQueryable<Product> GetAllProducts();
		IQueryable<Product> FindProducts(ProductSearchFilter filter);
		Task<bool> IsExistProduct(Guid id, CancellationToken ct);
	}
}
