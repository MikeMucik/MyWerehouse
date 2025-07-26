using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;


namespace MyWerehouse.Domain.Interfaces
{
	public interface IProductRepo
	{		
		Task<int> AddProductAsync(Product product);		
		Task DeleteProductByIdAsync(int id);		
		Task SwitchOffProductAsync(int id);		
		//Task UpdateProductAsync(Product product);		
		Task<Product?> GetProductByIdAsync(int name);		
		Task<Product?> GetProductToEditAsync(int name);		
		IQueryable<Product> GetAllProducts();		
		IQueryable<Product> FindProducts(ProductSearchFilter filter);		
	}
}
