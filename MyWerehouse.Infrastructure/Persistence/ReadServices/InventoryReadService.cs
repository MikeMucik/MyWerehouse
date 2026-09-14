using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.DTOs;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Infrastructure.Persistence.ReadServices
{
	public class InventoryReadService(WerehouseDbContext werehouseDbContext) : IInventoryReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public Task<PagedResult<InventoryDTO>> GetInventories(int pageNumber, int pageSize, CancellationToken ct)
		{
			var result = _werehouseDbContext.Inventories
				.AsNoTracking()
				.Select(i => new InventoryDTO {
					ProductId = i.ProductId,
					Quantity = i.Quantity,
					LastUpdated = i.LastUpdated,
				})
				.OrderBy(a=>a.ProductId);
			var resultToShow = result.ToPagedResultAsync(pageNumber, pageSize, ct);
			return resultToShow;
		}

		public Task<InventoryDTO?> GetInventory(Guid productId, CancellationToken ct)
		{
			var result = _werehouseDbContext.Inventories
				.AsNoTracking()
				.Where(i => i.ProductId == productId)
				.Select(i => new InventoryDTO
				{
					ProductId = i.ProductId,
					Quantity = i.Quantity,
					LastUpdated = i.LastUpdated,
				})
				.FirstOrDefaultAsync(ct);
			return result;
		}
	}
}
