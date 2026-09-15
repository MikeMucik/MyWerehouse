using System;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Inventories.Models;

namespace MyWerehouse.Application.Inventories.Events.ChangeStock
{
	public class ChangeStockHandler(IInventoryRepo inventoryRepo, IDateTimeProvider dateTimeProvider)
		: INotificationHandler<DomainEventNotification<ChangeStockNotification>>
	{
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		public async Task Handle(DomainEventNotification<ChangeStockNotification> notification, CancellationToken cancellationToken)
		{
			var domaintEvent = notification.DomainEvent;
			if (!domaintEvent.Changes.Any()) return;
			var productIds = domaintEvent.Changes.Select(c => c.ProductId).ToList();
			var inventories = await _inventoryRepo.GetInventoriesForProductsAsync(productIds, cancellationToken);
			var inventoryDict = inventories.ToDictionary(i => i.ProductId);

			foreach (var change in domaintEvent.Changes)
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
