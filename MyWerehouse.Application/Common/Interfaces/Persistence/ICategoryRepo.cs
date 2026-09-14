using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface ICategoryRepo
	{
		void AddCategory(Category category);
		void DeleteCategory(Category category);
		Task SwitchOffCategoryAsync(int idCategory, CancellationToken ct);
		Task<Category?> GetCategoryByIdAsync(int id, CancellationToken ct);
		Task<Category?> GetCategoryByNameAsync(string name, CancellationToken ct);
		
	}
}
