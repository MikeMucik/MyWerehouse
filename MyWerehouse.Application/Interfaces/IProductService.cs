using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductService
	{
		Task<AppResult<Guid>> AddProductAsync(EditProductDTO product);
		Task<AppResult<Unit>> DeleteProductAsync(Guid id);
		Task<AppResult<EditProductDTO>> GetProductToEditAsync(Guid id);
		Task<AppResult<Unit>> UpdateProductAsync(Guid id, EditProductDTO product);
		Task<AppResult<DetailsOfProductDTO>> DetailsOfProductAsync(Guid id);
		Task<AppResult<PagedResult<ProductDTO>>> GetProductsAsync(int pageNumber, int pageSize,CancellationToken ct);
		Task<AppResult<PagedResult<ProductDTO>>> FindProductsByFilterAsync(int pageNumber, int pageSize, ProductSearchFilter filter, CancellationToken ct);
	}
}
