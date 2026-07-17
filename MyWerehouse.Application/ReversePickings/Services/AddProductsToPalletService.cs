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
using MyWerehouse.Domain.Picking.Models;

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

		public async Task<ReversePickingResult> AddProductsToSourcePallet(ReversePicking reversePicking, string userId)
		{
			var sourcePallet = reversePicking.PickingTask.VirtualPallet?.Pallet;
			if (sourcePallet == null)
			{
				return ReversePickingResult.Fail("Paleta źródłowa nie istnieje.");
			}
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				sourcePallet.ProductsOnPallet.Single().IncreaseQuantity(reversePicking.Quantity);
			}
			else
			{
				return ReversePickingResult.Fail("Paleta źródłowa ma nieprawidłowy status.");
			}
			sourcePallet.AddHistory(ReasonForPallet.ReversePicking, userId, sourcePallet.Location.ToSnapshot());
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(sourcePallet.Id);
			virtualPallet?.ChangeToAvailable(userId, sourcePallet.Location.ToSnapshot());
			return ReversePickingResult.Ok("Dodano towar do palety źródłowej", reversePicking.ProductId, reversePicking.SourcePalletId);
		}
		public async Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, List<Pallet> pallets, string userId)
		{
			var quantityToAdded = task.Quantity;
			var product = await _productRepo.GetProductByIdAsync(task.ProductId);
			if (product == null)
			{
				return ReversePickingResult.Fail("Produkt nie istnieje.");
			}
			var cartonsOnPallet = product.CartonsPerPallet;
			if (pallets.Count == 0)
				return ReversePickingResult.Fail("Brak palet do uzupełnienia");
			if (pallets.Any(p => p.ProductsOnPallet.Single().Quantity >= cartonsOnPallet))
				return ReversePickingResult.Fail("Próba uzupełnienia pełnej palety");
			var listPalletToAddProduct = new List<PalletProductQuantityDTO>();
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
				pallet.AddHistory(ReasonForPallet.ReversePicking, userId, pallet.Location.ToSnapshot());
				var productToAdd = new PalletProductQuantityDTO
				{
					PalletId = pallet.Id,
					PalletNumber = pallet.PalletNumber,
					ProductId = product.Id,
					ProductName = product.Name,
					ProductSKU = product.SKU,
					Quantity = quantityToAdded,
				};
				listPalletToAddProduct.Add(productToAdd);
			}
			return ReversePickingResult.Ok("Dodano towar.", listPalletToAddProduct);
		}

		public async Task<ReversePickingResult> AddToNewPallet(ReversePicking task, string userId,int locationId, string snapShot)
		{
			var newNumber = await _palletRepo.GetNextPalletIdAsync();
			var now = _dateTimeProvider.UtcNow;
			var newPallet = Pallet.Create(newNumber, locationId, now);
			newPallet.AddProduct(task.ProductId, task.Quantity, now, task.BestBefore);
			newPallet.ChangeStatus(PalletStatus.InStock);
			_palletRepo.AddPallet(newPallet);
			newPallet.CreateNewPalletFromReservePicking(snapShot, userId);
			return ReversePickingResult.Ok("Dodano towar do nowej palety.", task.ProductId, newPallet.Id, newPallet.PalletNumber);
		}
	}
}
