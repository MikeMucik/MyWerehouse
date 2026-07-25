using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Inventories.Models;

namespace MyWerehouse.Application.Inventories.Events.ChangeStock
{
	public class ChangeStockHandler(IInventoryRepo inventoryRepo, IDateTimeProvider dateTimeProvider) : INotificationHandler<ChangeStockNotification>
	{
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
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
					var newInventory = Inventory.CreateStockItem(change.ProductId,
						change.Quantity, _dateTimeProvider.UtcNow);					
					_inventoryRepo.AddInventory(newInventory);
				}
				else
				{	
					inventory.ApplyChangeInInventory(change.Quantity, _dateTimeProvider.UtcNow);
				}				
			}
		}
	}
}
