using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Events;
using MyWerehouse.Domain.Invetories.InventoryExceptions;
using MyWerehouse.Domain.Invetories.Models;

namespace MyWerehouse.Application.Inventories.Events.ChangeStock
{
	public class ChangeStockHandler(IInventoryRepo inventoryRepo, IProductRepo productRepo) : INotificationHandler<ChangeStockNotification>
	{
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly IProductRepo _productRepo = productRepo;//for better errorInfo
		public async Task Handle(ChangeStockNotification notification, CancellationToken cancellationToken)
		{
			if (!notification.Changes.Any()) { return; }

			var productIds = notification.Changes.Select(c => c.ProductId).ToList();
			var inventories = await _inventoryRepo.GetInventoriesForProductsAsync(productIds);
			var inventoryDict = inventories.ToDictionary(i => i.ProductId);
			
			foreach (var change in notification.Changes)
			{
				inventoryDict.TryGetValue(change.ProductId, out var inventory);
				
				if (inventory == null)
				{
					var newInventory = new Inventory
					{
						ProductId = change.ProductId,						
						Quantity = change.Quantity,
						LastUpdated = DateTime.UtcNow,
					};
					_inventoryRepo.AddInventory(newInventory);
				}
				else
				{					
					inventory.Quantity += change.Quantity;
					if (inventory.Quantity < 0)
					{
						var product = await _productRepo.GetProductByIdAsync(change.ProductId);
						throw new DomainInventoryDomainException(change.ProductId, product.SKU);
					}
					inventory.LastUpdated = DateTime.UtcNow;
				}				
			}
		}
	}
}
