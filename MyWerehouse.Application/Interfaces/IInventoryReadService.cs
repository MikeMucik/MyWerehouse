using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Interfaces
{
	public interface IInventoryReadService
	{
		Task<InventoryDTO?> GetInventory(Guid productId, CancellationToken ct);
		Task<PagedResult<InventoryDTO>> GetInventories(int pageNumber, int pageSize, CancellationToken ct);
	}
}
