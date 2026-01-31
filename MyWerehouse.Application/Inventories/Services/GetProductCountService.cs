using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyWerehouse.Application.Inventories.Services
{
	public class GetProductCountService : IGetProductCountService
	{
		private readonly IInventoryRepo _inventoryRepo;
		public GetProductCountService(IInventoryRepo inventoryRepo)
		{
			_inventoryRepo = inventoryRepo;
		}

		public async Task<int> GetProductCountAsync(int productId, DateOnly? bestBefore)
		{
			var totalProductByDate = await _inventoryRepo.GetQuantityForProductAsync(productId, bestBefore);
			var totalProductReservedToIssues = await _inventoryRepo.GetQuantityProductReservedForIssueAsync(productId, bestBefore);
			var totalProductReservedToPicking = await _inventoryRepo.GetQuantityProductReservedForPickingAsync(productId, bestBefore);
			return totalProductByDate - totalProductReservedToIssues - totalProductReservedToPicking;
		}
	}
}
