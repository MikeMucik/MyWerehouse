using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.InventoryModels;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly IInventoryRepo _inventoryRepo;
		private readonly IMapper _mapper;

		public InventoryService(
			IInventoryRepo inventoryRepo
			, IMapper mapper
			)
		{
			_inventoryRepo = inventoryRepo;
			_mapper = mapper;
		}

		public async Task ChangeProductQunatityAsync(int productId, int quantity)
		{
			var inventory = _inventoryRepo.GetInventoryForProductAsync(productId);
			if( inventory == null )
			{
				await _inventoryRepo.AddInventoryAsync(productId, quantity);
			}
			if (quantity > 0)
			{
				await _inventoryRepo.IncreaseInventoryQuantityAsync(productId, quantity);
			}
			else
			{
				var productToDecrease = await _inventoryRepo.GetInventoryForProductAsync(productId);
				if (productToDecrease.Quantity < quantity)
				{
					throw new InvalidOperationException($"Brak odpowiedniej ilości produktu o numerze {productId}");
				}
				await _inventoryRepo.DecreaseInventoryQuantityAsync(productId, quantity);
			}
		}
		public async Task<InventoryDTO> GetInventoryAsync(int productId)
		{
			var inventory = await _inventoryRepo.GetInventoryForProductAsync(productId);
			var inventoryDTO = _mapper.Map<InventoryDTO>(inventory);
			return inventoryDTO;
		}

		public async Task<int> GetProductCountAsync(int productId, DateOnly? bestBefore)
		{
			var totalProductByDate =await _inventoryRepo.GetQuantityForProductAsync(productId, bestBefore);
			var totalProductReservedToIssues = await _inventoryRepo.GetQuantityProductReservedForIssueAsync(productId, bestBefore);
			var totalProductReservedToPicking =await _inventoryRepo.GetQuantityProductReservedForPickingAsync(productId, bestBefore);
			return totalProductByDate-totalProductReservedToIssues-totalProductReservedToPicking;
		}

		public async Task UpdateProductQunatityAsync(int productId, int quantity)
		{
			await _inventoryRepo.UpdateInventoryAsync(productId, quantity);
		}
	}
}
