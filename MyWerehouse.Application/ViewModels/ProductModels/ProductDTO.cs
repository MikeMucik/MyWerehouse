using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class ProductDTO
	{		
		public string Name { get; init; } = string.Empty;
		public string SKU { get; init; } = string.Empty ;
		public string Category { get; init; } = string.Empty;
	}
}
