using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Inventories.Events.ChangeStock
{
	public class ChangeStockHandler(IInventoryRepo inventoryRepo, WerehouseDbContext werehouseDbContext) : INotificationHandler<ChangeStockNotification>
	{
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task Handle(ChangeStockNotification notification, CancellationToken cancellationToken)
		{
			if (!notification.Changes.Any()) { return; }

			var isIncrease = notification.ChangeType == StockChangeType.Increase;

			var productIds = notification.Changes.Select(c => c.ProductId).ToList();
			var inventories = await _inventoryRepo.GetInventoriesForProductsAsync(productIds);
			var inventoryDict = inventories.ToDictionary(i => i.ProductId);
					
			foreach (var change in notification.Changes)
			{
				var absQuantity = Math.Abs(change.Quantity);
				inventoryDict.TryGetValue(change.ProductId, out var inventory);
				if (isIncrease)
				{
					if (inventory == null)
					{
						var newInventory = new Inventory
						{
							ProductId = change.ProductId,
							Quantity = absQuantity,
							LastUpdated = DateTime.UtcNow,
						};
						_inventoryRepo.AddInventory(newInventory); 
					}
					else
					{
						inventory.Quantity += absQuantity;
						inventory.LastUpdated = DateTime.UtcNow;
					}
				}
				else 
				{
					if (inventory == null)
					{						
						// throw new InventoryException(...);
						continue;
					}
					//if (inventory.Quantity < absQuantity)
					//{
					//	// j.w.
					//	// throw new InventoryException(...);
					//	continue;
					//}
					inventory.Quantity -= absQuantity;
					inventory.LastUpdated = DateTime.UtcNow;
				}
			}
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
