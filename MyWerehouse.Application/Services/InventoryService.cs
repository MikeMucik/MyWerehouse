using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.DTOs;
using MyWerehouse.Application.Inventories.Queries.GetInventory;
using MyWerehouse.Application.Inventories.Queries.GetProductCount;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Application.Inventories.Services;

namespace MyWerehouse.Application.Services
{
	public class InventoryService : IInventoryService
	{		
		private readonly IMediator _mediator;
		private readonly IGetProductCountService _getProductCountService;

		public InventoryService(			
			IMediator mediator,
			IGetProductCountService getProductCountService)
		{			
			_mediator = mediator;
			_getProductCountService = getProductCountService;
		}

		public async Task ChangeProductQuantityAsync(int productId, int quantity)
		{
			//return _mediator.Publish(new ChangeStockNotification()
			//await _mediator.Send(new ChangeQuantityCommand(productId, quantity));
			//return;
			//if(quantity == 0) {return; }
			//var absQuantity = Math.Abs(quantity);
			//var isIncrease = quantity > 0;
			//var inventory = await _inventoryRepo.GetInventoryForProductAsync(productId);

			//if (isIncrease)
			//{
			//	if (inventory == null)
			//	{
			//		 inventory = new Inventory
			//		{
			//			ProductId = productId,
			//			Quantity = absQuantity,
			//			LastUpdated = DateTime.UtcNow,
			//		};
			//		_inventoryRepo.AddInventory(inventory);
			//	}
			//	else
			//	{
			//				inventory.Quantity += absQuantity;
			//				inventory.LastUpdated = DateTime.UtcNow;
			//	}
			//}
			//else
			//{
			//	if (inventory == null)
			//	{
			//		throw new InventoryException($"Brak inventory dla produktu {productId}. Nie można zmniejszyć stanu."); //tekst do zmiany
			//	}

			//	if (inventory.Quantity < absQuantity)
			//	{
			//		throw new InventoryException($"Niewystarczająca ilość w magazynie dla produktu o ID: {productId}. Dostępne: {inventory.Quantity}, żądane: {absQuantity}.");
			//	}
			//	inventory.Quantity -= absQuantity;
			//	inventory.LastUpdated = DateTime.UtcNow;
			//}
			//await _werehouseDbContext.SaveChangesAsync();
		}

		public async Task<InventoryDTO> GetInventoryAsync(int productId)
		{
			return await _mediator.Send(new GetInventoryQuery(productId));
			//var inventory = await _inventoryRepo.GetInventoryForProductAsync(productId);
			//var inventoryDTO = _mapper.Map<InventoryDTO>(inventory);
			//return inventoryDTO;
		}
		public async Task<int> GetProductCountAsync(int productId, DateOnly? bestBefore)
		{
			//return	await _mediator.Send(new GetProductCountQuery(productId, bestBefore));
			return	await _getProductCountService.GetProductCountAsync( productId, bestBefore);
			//var totalProductByDate = await _inventoryRepo.GetQuantityForProductAsync(productId, bestBefore);
			//var totalProductReservedToIssues = await _inventoryRepo.GetQuantityProductReservedForIssueAsync(productId, bestBefore);
			//var totalProductReservedToPicking = await _inventoryRepo.GetQuantityProductReservedForPickingAsync(productId, bestBefore);
			//return totalProductByDate - totalProductReservedToIssues - totalProductReservedToPicking;
		}		
	}
}
