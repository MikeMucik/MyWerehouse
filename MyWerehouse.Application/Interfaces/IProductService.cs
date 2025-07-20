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
		int AddProduct(AddProductDTO product);
		Task<int> AddProductAsync(AddProductDTO product);
		void DeleteProduct(int productId);
		Task DeleteProductAsync(int productId);
		AddProductDTO GetProductToEdit(int productId);
		Task<AddProductDTO> GetProductToEditAsync(int productId);
		void UpdateProduct(AddProductDTO product);
		Task UpdateProductAsync(AddProductDTO product);
		DetailsOfProductDTO DetailsOfProduct(int productId);
		Task<DetailsOfProductDTO> DetailsOfProductAsync(int productId);
		ListProductsDTO GetProducts(int pageSize, int PageNumber);
		Task <ListProductsDTO> GetProductsAsync(int pageSize, int PageNumber);
		ListProductsDTO FindProductsByFilter(int pageSize, int PageNumber, ProductSearchFilter filter);
		Task <ListProductsDTO> FindProductsByFilterAsync(int pageSize, int PageNumber, ProductSearchFilter filter);
		
	}
}
