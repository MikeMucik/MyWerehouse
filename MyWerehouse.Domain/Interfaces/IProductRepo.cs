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
		int AddProduct(Product product);
		Task<int> AddProductAsync(Product product);
		void DeleteProductById(int id);
		Task DeleteProductByIdAsync(int id);
		void SwitchOffProduct(int id);
		Task SwitchOffProductAsync(int id);
		void UpdateProduct(Product product);
		Task UpdateProductAsync(Product product);
		Product? GetProductById(int id);
		Product? GetProductToEdit(int id);
		Task<Product?> GetProductByIdAsync(int name);		
		Task<Product?> GetProductToEditAsync(int name);		
		IQueryable<Product> GetAllProducts();		
		IQueryable<Product> FindProducts(ProductSearchFilter filter);		
	}
}
