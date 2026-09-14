using MyWerehouse.Domain.Products.Models;


namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IProductRepo
	{
		Product AddProduct(Product product);
		void DeleteProduct(Product product);
		Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct);
		Task<string?> GetSKUForProductAsync(Guid id, CancellationToken ct);
		Task<Product?> GetProductToEditAsync(Guid id, CancellationToken ct);
		Task<bool> AlreadyExist(string name, string sku);
		Task<bool> IsExistProduct(Guid id, CancellationToken ct);
		Task<bool> HasProductsInCategory(int categoryId,CancellationToken ct);
	}
}
