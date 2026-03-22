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
		Task<Product?> GetProductByIdAsync(Guid id);		
		Task<Product?> GetProductToEditAsync(Guid id);		
		IQueryable<Product> GetAllProducts();		
		IQueryable<Product> FindProducts(ProductSearchFilter filter);		
		Task<bool> IsExistProduct(Guid id);
		//Task<bool> EnsureAllExist(List<int> ids);
	}
}
