using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductService
	{
		Task<AppResult<int>> AddProductAsync(AddProductDTO product);
		Task<AppResult<Unit>> DeleteProductAsync(int productId);
		Task<AppResult<AddProductDTO>> GetProductToEditAsync(int productId);
		Task<AppResult<Unit>> UpdateProductAsync(AddProductDTO product);
		Task<AppResult<DetailsOfProductDTO>> DetailsOfProductAsync(int productId);
		Task<AppResult<ListProductsDTO>> GetProductsAsync(int pageSize, int PageNumber);
		Task<AppResult<ListProductsDTO>> FindProductsByFilterAsync(int pageSize, int PageNumber, ProductSearchFilter filter);
	}
}
