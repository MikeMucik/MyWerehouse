using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductReadService
	{
		Task<EditProductDTO?> GetProductToEditAsync(Guid id, CancellationToken ct);
		Task<DetailsOfProductDTO?> DetailsOfProductAsync(Guid id, CancellationToken ct);
		Task<PagedResult<ProductDTO>> GetProductsAsync(int pageNumber, int pageSize, CancellationToken ct);
		Task<PagedResult<ProductDTO>> FindProductsByFilterAsync(int pageNumber, int pageSize, ProductSearchFilter filter, CancellationToken ct);
	}
}
