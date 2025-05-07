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
		Product GetProductById(int id);
		void UpdateProduct(Product product);
		void DeleteProductById(int id);
		void SwitchOffProduct(int id);
		IQueryable<Product> GetAllProducts();
		IQueryable<Product> FindProducts(ProductSearchFilter filter);
	}
}
