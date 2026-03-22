using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Interfaces
{
	public interface IInventoryService
	{
		Task ChangeProductQuantityAsync(Guid productId, int quantity);
		Task<InventoryDTO> GetInventoryAsync(Guid productId);
		Task<int> GetProductCountAsync(Guid productId, DateOnly? BestBefore);
	}
}
