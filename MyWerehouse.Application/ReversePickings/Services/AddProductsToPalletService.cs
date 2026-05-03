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
	public class AddProductsToPalletService(
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		ILocationRepo locationRepo,
		IVirtualPalletRepo virtualPalletRepo) : IAddProductsToPalletService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;

		public async Task<ReversePickingResult> AddProductsToSourcePallet(ReversePicking reversePicking, string userId)
		{
			var sourcePallet = reversePicking.PickingTask.VirtualPallet?.Pallet;//
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				sourcePallet.ProductsOnPallet.Single().IncreaseQuantity(reversePicking.Quantity);
			}
			else
			{
				return ReversePickingResult.Fail("Paleta źródłowa ma nieprawidłowy status.");
			}
			sourcePallet.AddHistory(ReasonMovement.ReversePicking, userId, sourcePallet.Location.ToSnapshot());
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(sourcePallet.Id);
			virtualPallet?.ChangeToAvailable(userId, sourcePallet.Location.ToSnapshot());
			return ReversePickingResult.Ok("Dodano towar do palety źródłowej", reversePicking.ProductId, reversePicking.SourcePalletId);
		}
		public async Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, List<Pallet> pallets, string userId)
		{
			var quantityToAdded = task.Quantity;
			var product = await _productRepo.GetProductByIdAsync(task.ProductId);
			var cartonsOnPallet = product.CartonsPerPallet;//
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
				pallet.ProductsOnPallet.Single().IncreaseQuantity(addedAmount);
				quantityToAdded -= addedAmount;
				pallet.AddHistory(ReasonMovement.ReversePicking, userId, pallet.Location.ToSnapshot());
			}
			return ReversePickingResult.Ok("Dodano towar.", pallets); //tu potrzebna pełna rozpiska ile towaru na daną paletę
		}

		public async Task<ReversePickingResult> AddToNewPallet(ReversePicking task, string userId, int rampNumber)
		{
			var newNumber = await _palletRepo.GetNextPalletIdAsync();
			var newPallet = Pallet.Create(newNumber, rampNumber);
			newPallet.AddProduct(task.ProductId, task.Quantity, task.BestBefore);
			newPallet.ChangeStatus(PalletStatus.InStock);
			var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
			var snapShot = location.ToSnapshot();
			_palletRepo.AddPallet(newPallet);
			newPallet.CreateNewPalletFromReservePicking(location.Id, snapShot, userId);
			return ReversePickingResult.Ok("Dodano towar do nowej palety.", task.ProductId, newPallet.Id, newPallet.PalletNumber);
		}
	}
}