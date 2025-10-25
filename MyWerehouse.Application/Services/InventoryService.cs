using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.InventoryModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly IInventoryRepo _inventoryRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;

		public InventoryService(
			IInventoryRepo inventoryRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext
			)
		{
			_inventoryRepo = inventoryRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task ChangeProductQuantityAsync(int productId, int quantity)
		{
			if(quantity == 0) {return; }
			var absQuantity = Math.Abs(quantity);
			var isIncrease = quantity > 0;
			var inventory = await _inventoryRepo.GetInventoryForProductAsync(productId);

			if (isIncrease)
			{
				if (inventory == null)
				{
					 inventory = new Inventory
					{
						ProductId = productId,
						Quantity = absQuantity,
						LastUpdated = DateTime.UtcNow,
					};
					_inventoryRepo.AddInventory(inventory);
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
					throw new InventoryException($"Brak inventory dla produktu {productId}. Nie można zmniejszyć stanu."); //tekst do zmiany
				}

				if (inventory.Quantity < absQuantity)
				{
					throw new InventoryException($"Niewystarczająca ilość w magazynie dla produktu o ID: {productId}. Dostępne: {inventory.Quantity}, żądane: {absQuantity}.");
				}
				inventory.Quantity -= absQuantity;
				inventory.LastUpdated = DateTime.UtcNow;
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<InventoryDTO> GetInventoryAsync(int productId)
		{
			var inventory = await _inventoryRepo.GetInventoryForProductAsync(productId);
			var inventoryDTO = _mapper.Map<InventoryDTO>(inventory);
			return inventoryDTO;
		}
		public async Task<int> GetProductCountAsync(int productId, DateOnly? bestBefore)
		{
			var totalProductByDate = await _inventoryRepo.GetQuantityForProductAsync(productId, bestBefore);
			var totalProductReservedToIssues = await _inventoryRepo.GetQuantityProductReservedForIssueAsync(productId, bestBefore);
			var totalProductReservedToPicking = await _inventoryRepo.GetQuantityProductReservedForPickingAsync(productId, bestBefore);
			return totalProductByDate - totalProductReservedToIssues - totalProductReservedToPicking;
		}
		public async Task UpdateProductQuantityAsync(int productId, int quantity)
		{
			if (quantity < 0)
			{
				throw new InventoryException($"Ilość nie może być ujemna dla produktu o ID: {productId}.");
			}

			var inventory = await _inventoryRepo.GetInventoryForProductAsync(productId)
				?? throw new InventoryException($"Brak inventory dla produktu o ID: {productId}.");

			inventory.Quantity = quantity;
			inventory.LastUpdated = DateTime.UtcNow;
			await _werehouseDbContext.SaveChangesAsync();
		}
	}
}
