using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Server.ServicesToInfrastructure
{
	public class CategoryReadService(WerehouseDbContext werehouseDbContext
			) : ICategoryReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<PagedResult<CategoryViewDTO>> GetCategoriesAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var categories = _werehouseDbContext.Categories
				.AsNoTracking()
				.Where(x=>x.IsDeleted == false)
				.OrderBy(n => n.Name)
				.Select(x => new CategoryViewDTO
				{
					Id = x.Id,
					Name = x.Name,
				});

			var result = await categories.ToPagedResultAsync(pageNumber, pageSize, ct);

			return result;
		}
	}
}
