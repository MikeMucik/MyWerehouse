using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.CategoryModels
{
	public class CategoryViewDTO 
	{
		public int Id { get; init; }
		public required string Name { get; init; }		
	}
}
