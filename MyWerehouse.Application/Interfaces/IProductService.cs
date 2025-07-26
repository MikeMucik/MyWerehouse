using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductService
	{		
		Task<int> AddProductAsync(AddProductDTO product);		
		Task DeleteProductAsync(int productId);		
		Task<AddProductDTO> GetProductToEditAsync(int productId);		
		Task UpdateProductAsync(AddProductDTO product);	
		Task<DetailsOfProductDTO> DetailsOfProductAsync(int productId);		
		Task <ListProductsDTO> GetProductsAsync(int pageSize, int PageNumber);		
		Task <ListProductsDTO> FindProductsByFilterAsync(int pageSize, int PageNumber, ProductSearchFilter filter);
	}
}
