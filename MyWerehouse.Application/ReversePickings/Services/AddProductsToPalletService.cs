using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public class AddProductsToPalletService : IAddProductsToPalletService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly ILocationRepo _locationRepo;
		public AddProductsToPalletService(
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			ILocationRepo locationRepo)
		{			
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_locationRepo = locationRepo;
		}
					
		public ReversePickingResult AddProductsToSourcePallet(ReversePicking reversePicking, string userId)
		{
			var sourcePallet = reversePicking.PickingTask.VirtualPallet.Pallet;
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				sourcePallet.ProductsOnPallet.Single().AddQuantity(reversePicking.Quantity);
				//sourcePallet.ProductsOnPallet.Single().Quantity += reversePicking.Quantity;
			}
			else
				return ReversePickingResult.Fail("Paleta źródłowa ma nieprawidłowy status.");
			sourcePallet.AddHistory(sourcePallet.Status, ReasonMovement.ReversePicking, userId);
			
			return ReversePickingResult.Ok("Dodano towar do palety źródłowej", reversePicking.ProductId, reversePicking.SourcePalletId);
		}
		public async Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, List<Pallet> pallets, string userId)
		{
			var quantityToAdded = task.Quantity;
			var product = await _productRepo.GetProductByIdAsync(task.ProductId);
			var cartonsOnPallet = product.CartonsPerPallet;
			if (pallets.Count == 0)
				return ReversePickingResult.Fail("Brak palet do uzupełnienia");
			if (pallets.Any(p => p.ProductsOnPallet.Single().Quantity >= cartonsOnPallet))
				return ReversePickingResult.Fail("Próba uzupełnienia pełnej palety");				
			foreach (var pallet in pallets)
			{
				if (quantityToAdded <= 0)
					break;
				var quantityOnPallet = pallet.ProductsOnPallet.Single().Quantity;
				var freeSpace = cartonsOnPallet - quantityOnPallet;
				if (freeSpace <= 0) continue;
				var addedAmount = Math.Min(quantityToAdded, freeSpace);
				pallet.ProductsOnPallet.Single().AddQuantity(addedAmount);
				//pallet.ProductsOnPallet.Single().Quantity += addedAmount;
				quantityToAdded -= addedAmount;
				pallet.AddHistory(pallet.Status, ReasonMovement.ReversePicking, userId);				
			}			
			return ReversePickingResult.Ok("Dodano towar.", pallets);
		}

		public async Task<ReversePickingResult> AddToNewPallet(ReversePicking task, string userId, int rampNumber)
		{
			var newNumber = await _palletRepo.GetNextPalletIdAsync();
			var newPallet = Pallet.Create(newNumber);
			newPallet.AddProduct(task.ProductId, task.Quantity, task.BestBefore);
			newPallet.ChangeStatus(PalletStatus.InStock);
			var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
			newPallet.AddLocation(location);
			//var newPallet = new Pallet
			//{
			//	PalletNumber = newNumber,
			//	DateReceived = DateTime.UtcNow,
			//	LocationId = 100100,
			//	Status = PalletStatus.InStock,				
			//	//ReceiptId = 1000,//to trzeba poprawić żeby taka nowa paleta miała jakieś przyjęcie tylko palety kompletacyjne nie mają ReceiptId
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{new ProductOnPallet
			//		{
			//			ProductId = task.ProductId,
			//			DateAdded = DateTime.UtcNow,
			//			Quantity = task.Quantity,
			//		 },
			//	},
			//};
			_palletRepo.AddPallet(newPallet);
			newPallet.AddHistory(PalletStatus.InStock, ReasonMovement.ReversePicking, userId);
			return ReversePickingResult.Ok("Dodano towar do nowej palety.", task.ProductId, newPallet.Id, newPallet.PalletNumber);
		}
	}
}
