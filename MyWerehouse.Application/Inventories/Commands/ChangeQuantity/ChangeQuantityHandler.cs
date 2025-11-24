using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Inventories.Commands.ChangeQuantity
{
	public class ChangeQuantityHandler(IInventoryRepo inventoryRepo, WerehouseDbContext werehouseDbContext) : INotificationHandler<ChangeQuantityCommand>
	{
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task Handle(ChangeQuantityCommand command, CancellationToken cancellationToken)
		{
			if (command.Quantity == 0) { return; }
			var absQuantity = Math.Abs(command.Quantity);
			var isIncrease = command.Quantity > 0;
			var inventory = await _inventoryRepo.GetInventoryForProductAsync(command.ProductId);
			if (isIncrease)
			{
				if (inventory == null)
				{
					var newInventory = new Inventory 
					{
						ProductId = command.ProductId,
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
					throw new InventoryException($"Brak inventory dla produktu {command.ProductId}. Nie można zmniejszyć stanu."); //tekst do zmiany
				}
				if (inventory.Quantity < absQuantity)
				{
					throw new InventoryException($"Niewystarczająca ilość w magazynie dla produktu o ID: {command.ProductId}. Dostępne: {inventory.Quantity}, żądane: {absQuantity}.");
				}
				inventory.Quantity -= absQuantity;
				inventory.LastUpdated = DateTime.UtcNow;
			}
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
