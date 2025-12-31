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
		Task ChangeProductQuantityAsync(int productId, int quantity);
		Task<InventoryDTO> GetInventoryAsync(int productId);
		Task<int> GetProductCountAsync(int productId, DateOnly? BestBefore);
	}
}
