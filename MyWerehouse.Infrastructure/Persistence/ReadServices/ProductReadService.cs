using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Infrastructure.Persistence.ReadServices
{
	public class ProductReadService(WerehouseDbContext werehouseDbContext) : IProductReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		public Task<DetailsOfProductDTO?> DetailsOfProductAsync(Guid id, CancellationToken ct)
		{
			var product = _werehouseDbContext.Products
				.AsNoTracking()
				.Include(pp => pp.Details)
				.Where(p => p.Id == id)
				.Select(p => new DetailsOfProductDTO
				{
					Id = p.Id,
					Name = p.Name,
					CategoryId = p.CategoryId,
					CategoryName = p.Category.Name,
					CartonsPerPallet = p.CartonsPerPallet,
					Height = p.Details.Height,
					Width = p.Details.Width,
					Length = p.Details.Length,
					Weight = p.Details.Weight,
					Description = p.Details.Description,
				})
				.FirstOrDefaultAsync(ct);
			return product;
		}

		public Task<PagedResult<ProductDTO>> FindProductsByFilterAsync(int pageNumber, int pageSize, ProductSearchFilter filter, CancellationToken ct)
		{
			var result = _werehouseDbContext.Products
				.Where(p => !p.IsDeleted);
			if (!string.IsNullOrEmpty(filter.ProductName))
			{
				result = result.Where(p => p.Name != null && p.Name.StartsWith(filter.ProductName));
			}
			if (!string.IsNullOrEmpty(filter.SKU))
			{
				result = result.Where(p => p.SKU != null && p.SKU.StartsWith(filter.SKU));
			}
			if (!string.IsNullOrEmpty(filter.Category))
			{
				result = result.Where(p => p.Category.Name != null && p.Category.Name.StartsWith(filter.Category));
			}
			if (filter.CategoryId > 0)
			{
				result = result.Where(p => p.CategoryId == filter.CategoryId);
			}
			if (filter.Height.HasValue && filter.Height > 0)
			{
				result = result.Where(p =>
				p.Details != null &&
				p.Details.Height == filter.Height);
			}
			if (filter.Weight.HasValue && filter.Weight > 0)
			{
				result = result.Where(p =>
				p.Details != null &&
				p.Details.Weight == filter.Weight);
			}
			if (filter.Width.HasValue && filter.Width > 0)
			{
				result = result.Where(p =>
				p.Details != null &&
				p.Details.Width == filter.Width);
			}
			if (filter.Length.HasValue && filter.Length > 0)
			{
				result = result.Where(p =>
				p.Details != null &&
				p.Details.Length == filter.Length);
			}
			var resultToShow = result
				.AsNoTracking()
				.OrderBy(p => p.SKU)
				.Select(p => new ProductDTO
				{
					SKU = p.SKU,
					Category = p.Category.Name,
					Name = p.Name
				});

			return resultToShow.ToPagedResultAsync(pageNumber, pageSize, ct);
		}

		public Task<PagedResult<ProductDTO>> GetProductsAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var result = _werehouseDbContext.Products
				.AsNoTracking()
				.Where(p => !p.IsDeleted)
				.OrderBy(p => p.SKU)
				.Select(p => new ProductDTO
				{
					SKU = p.SKU,
					Category = p.Category.Name,
					Name = p.Name
				});
			return result.ToPagedResultAsync(pageNumber, pageSize, ct);
		}

		public async Task<EditProductDTO?> GetProductToEditAsync(Guid id, CancellationToken ct)
		{
			var result = await _werehouseDbContext.Products
				.AsNoTracking()
				.Include(pp => pp.Details)
				.Where(p => !p.IsDeleted)
				.Where(p => p.Id == id)
				.Select(product => new EditProductDTO
				{
					Id = product.Id,
					SKU = product.SKU,
					Name = product.Name,
					CategoryId = product.CategoryId,
					CartonsPerPallet = product.CartonsPerPallet,
					Description = product.Details.Description,
					Length = product.Details.Length,
					Width = product.Details.Width,
					Height = product.Details.Height,
					Weight = product.Details.Weight,
				})
				.FirstOrDefaultAsync(ct);
			return result;
		}
	}
}
