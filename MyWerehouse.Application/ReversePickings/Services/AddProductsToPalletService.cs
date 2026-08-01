using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public class AddProductsToPalletService(
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		IVirtualPalletRepo virtualPalletRepo,
		IDateTimeProvider dateTimeProvider) : IAddProductsToPalletService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<ReversePickingResult> AddProductsToSourcePallet(ReversePickingTask reversePicking, string userId)
		{
			var sourcePallet = reversePicking.PickingTask.VirtualPallet?.Pallet;
			if (sourcePallet == null)
			{
				return ReversePickingResult.Fail("Source pallet does not exist.");
			}
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				sourcePallet.ProductsOnPallet.Single().IncreaseQuantity(reversePicking.Quantity);
			}
			else
			{
				return ReversePickingResult.Fail("Source pallet has an invalid status.");
			}
			sourcePallet.AddHistory(ReasonForPallet.ReversePicking, userId, sourcePallet.Location.ToSnapshot());
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(sourcePallet.Id);
			if (virtualPallet != null)
			{
				var availabilityPallet = virtualPallet.ChangeToAvailable(userId, sourcePallet.Location.ToSnapshot());
				if (availabilityPallet)
				{
					_virtualPalletRepo.DeleteVirtualPalletPicking(virtualPallet);
				}
			}
			return ReversePickingResult.Ok("Product was returned to the source pallet.", reversePicking.ProductId, reversePicking.SourcePalletId);
		}
		public async Task<ReversePickingResult> AddToExistingPallet(ReversePickingTask task,
			//List<Pallet> pallets,
			List<Guid> pallets,
			string userId)
		{
			var quantityToAdded = task.Quantity;
			var product = await _productRepo.GetProductByIdAsync(task.ProductId);
			if (product == null)
			{
				return ReversePickingResult.Fail("Product does not exist.");
			}
			var cartonsOnPallet = product.CartonsPerPallet;
			if (pallets.Count == 0)
				return ReversePickingResult.Fail("No pallets are available for replenishment.");

			//if (pallets.Any(p => p.ProductsOnPallet.Single().Quantity >= cartonsOnPallet))
			//	return ReversePickingResult.Fail("Cannot replenish a full pallet.");

			var listPalletToAddProduct = new List<PalletProductQuantityDTO>();
			foreach (var pallet in pallets)
			{
				if (quantityToAdded <= 0)
					break;
				var palletToAdd = await _palletRepo.GetPalletByIdAsync(pallet);
				if (palletToAdd == null)
				{
					return ReversePickingResult.Fail("Problem with pallet, pallet missing.");
				}
				var resultAdding = palletToAdd.AddReversePickedProduct(task.ProductId, task.BestBefore, quantityToAdded, cartonsOnPallet, userId, palletToAdd.Location.ToSnapshot());
				quantityToAdded = resultAdding.Item1;
				//var quantityOnPallet = pallet.ProductsOnPallet.Single().Quantity;
				//var freeSpace = cartonsOnPallet - quantityOnPallet;
				//if (freeSpace <= 0) continue;
				//var addedAmount = Math.Min(quantityToAdded, freeSpace);
				//pallet.ProductsOnPallet.Single().IncreaseQuantity(addedAmount);
				//quantityToAdded -= addedAmount;
				//pallet.AddHistory(ReasonForPallet.ReversePicking, userId, pallet.Location.ToSnapshot());
				
				var productToAdd = new PalletProductQuantityDTO
				{
					PalletId = pallet,
					PalletNumber = palletToAdd.PalletNumber,
					ProductId = product.Id,
					ProductName = product.Name,
					ProductSKU = product.SKU,
					Quantity = resultAdding.Item2,
				};
				listPalletToAddProduct.Add(productToAdd);
			}
			if (quantityToAdded > 0)
			{
				return ReversePickingResult.Fail("Product was not added.");
			}
			return ReversePickingResult.Ok("Product was added.", listPalletToAddProduct);
		}

		public async Task<ReversePickingResult> AddToNewPallet(ReversePickingTask task, string userId, int locationId, string snapShot)
		{
			var newNumber = await _palletRepo.GetNextPalletIdAsync();
			var now = _dateTimeProvider.UtcNow;
			var newPallet = Pallet.Create(newNumber, locationId, now);
			newPallet.AddProduct(task.ProductId, task.Quantity, now, task.BestBefore);
			newPallet.ChangeStatus(PalletStatus.InStock);
			_palletRepo.AddPallet(newPallet);
			newPallet.CreateNewPalletFromReservePicking(snapShot, userId);
			return ReversePickingResult.Ok("Product was added to a new pallet.", task.ProductId, newPallet.Id, newPallet.PalletNumber);
		}
	}
}
