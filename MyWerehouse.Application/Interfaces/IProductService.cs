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
		void DeleteProduct(int productId);
		AddProductDTO GetProductToEdit(int productId);
		void UpdateProduct(AddProductDTO product);
		DetailsOfProductDTO DetailsOfProduct(int productId);
		ListProductsDTO GetProducts(int pageSize, int PageNumber);
		ListProductsDTO FindProductsByFilter(int pageSize, int PageNumber, ProductSearchFilter filter);
	}
}
