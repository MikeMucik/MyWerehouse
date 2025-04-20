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
		bool UpdateProduct(Product product);
		bool DeleteProductById(int id);
		IQueryable<Product> GetAllProducts();
		IQueryable< Product> FindProduct(string productName, string SKU, int Length, int Height, int Width, int Weight);
	}
}
