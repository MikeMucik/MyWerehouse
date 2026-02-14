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
		void SwitchOffProduct(Product product);					
		//Task SwitchOffProductAsync(int id);					
		Task<Product?> GetProductByIdAsync(int name);		
		Task<Product?> GetProductToEditAsync(int name);		
		IQueryable<Product> GetAllProducts();		
		IQueryable<Product> FindProducts(ProductSearchFilter filter);		
		Task<bool> IsExistProduct(int productId);
		//Task<bool> EnsureAllExist(List<int> ids);
	}
}
