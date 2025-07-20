using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.InventoryModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface IInventoryService
	{
		Task ChangeProductQunatityAsync(int productId, int quantity);
		Task UpdateProductQunatityAsync(int productId, int quantity);
		Task<InventoryDTO> GetInventoryAsync(int productId);
	}
}
