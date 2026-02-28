using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public class AddProductsToPalletService : IAddProductsToPalletService
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IEventCollector _eventCollector;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		public AddProductsToPalletService(WerehouseDbContext werehouseDbContext,
			IReversePickingRepo reversePickingRepo,
			IEventCollector eventCollector,
			IPalletRepo palletRepo,
			IProductRepo productRepo)
		{
			 _reversePickingRepo = reversePickingRepo;
			_werehouseDbContext	= werehouseDbContext;
			_eventCollector = eventCollector;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
		}
					
		public ReversePickingResult AddProductsToSourcePallet(ReversePicking reversePicking, string userId)
		{
			var sourcePallet = reversePicking.PickingTask.VirtualPallet.Pallet;
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				sourcePallet.ProductsOnPallet.Single().Quantity += reversePicking.Quantity;
			}
			else throw new NotFoundPalletException("Paleta źródłowa ma nieprawidłowy status.");
			sourcePallet.AddHistory(sourcePallet.Status, ReasonMovement.ReversePicking, userId);
			
			return ReversePickingResult.Ok("Dodano towar do palety źródłowej", reversePicking.ProductId, reversePicking.SourcePalletId);
		}
		public async Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, List<Pallet> pallets, string userId)
		{
			var quantityToAdded = task.Quantity;
			var product = await _productRepo.GetProductByIdAsync(task.ProductId)
				?? throw new NotFoundProductException(task.ProductId);
			var cartonsOnPallet = product.CartonsPerPallet;
			if (pallets.Count == 0)
				throw new NotFoundPalletException("Brak palet do uzupełnienia");
			if (pallets.Any(p => p.ProductsOnPallet.Single().Quantity >= cartonsOnPallet))
				throw new NotFoundPalletException("Próba uzupełnienia pełnej palety");				
			foreach (var pallet in pallets)
			{
				if (quantityToAdded <= 0)
					break;
				var quantityOnPallet = pallet.ProductsOnPallet.Single().Quantity;
				var freeSpace = cartonsOnPallet - quantityOnPallet;
				if (freeSpace <= 0) continue;
				var addedAmount = Math.Min(quantityToAdded, freeSpace);
				pallet.ProductsOnPallet.Single().Quantity += addedAmount;
				quantityToAdded -= addedAmount;
				pallet.AddHistory(pallet.Status, ReasonMovement.ReversePicking, userId);				
			}			
			return ReversePickingResult.Ok("Dodano towar.", pallets);
		}

		public async Task<ReversePickingResult> AddToNewPallet(ReversePicking task, string userId)
		{
			var newNumber = await _palletRepo.GetNextPalletIdAsync();
			var newPallet = new Pallet
			{
				Id = newNumber,
				DateReceived = DateTime.UtcNow,
				LocationId = 100100,
				Status = PalletStatus.InStock,				
				//ReceiptId = 1000,//to trzeba poprawić żeby taka nowa paleta miała jakieś przyjęcie tylko palety kompletacyjne nie mają ReceiptId
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
					{
						ProductId = task.ProductId,
						DateAdded = DateTime.UtcNow,
						Quantity = task.Quantity,
					 },
				},
			};
			_palletRepo.AddPallet(newPallet);
			newPallet.AddHistory(PalletStatus.InStock, ReasonMovement.ReversePicking, userId);
			return ReversePickingResult.Ok("Dodano towar do nowej palety.", task.ProductId, newPallet.Id);
		}
	}
}
