using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Picking.Services
{
	public class ExecuteProcessPickingService(IPalletRepo palletRepo,
		ILocationRepo locationRepo, IProductRepo productRepo, IDateTimeProvider dateTimeProvider) : IExecuteProcessPickingService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<ProcessPickingActionResult> ExecuteProcessPicking(Pallet sourcePallet, PickingTask pickingTask,
			int quantityToPick, string userId, int locationId)
		{
			var location = await _locationRepo.GetLocationByIdAsync(locationId);
			if (location == null) return ProcessPickingActionResult.Fail("Ramp was not found.");
			var snapshotPickingPallet = location.ToSnapshot();
			
			var productOnSourcePallet = sourcePallet.GetProductOnPallet(pickingTask.ProductId, pickingTask.BestBefore);
			var pickingPallet = await GetOrCreatePickingPallet(pickingTask.IssueId, pickingTask.ProductId, quantityToPick, userId,
				pickingTask, locationId, snapshotPickingPallet, sourcePallet, productOnSourcePallet.BestBefore);
			var productSKU = await _productRepo.GetSKUForProductAsync(pickingTask.ProductId);
			var snapshotSourcePallet = sourcePallet.Location.ToSnapshot();
			sourcePallet.PickProduct(productOnSourcePallet, quantityToPick, userId, snapshotSourcePallet);
			if (pickingPallet.NewPalletCreated)
			{
				return ProcessPickingActionResult.OkWithNewPallet(pickingPallet.PalletId, pickingPallet.PalletNumber,
					$"Take a new pallet for the issue. Product: {productSKU}, quantity: {quantityToPick}.");
			}
			else
			{
				return ProcessPickingActionResult.Ok(pickingPallet.PalletId, pickingPallet.PalletNumber,
					$"Add the product to the existing picking pallet. Product: {productSKU}, quantity: {quantityToPick}.");
			}
		}
		
		public async Task<CreateNewPickingPalletResult> GetOrCreatePickingPallet(Guid issueId, Guid productId, int quantity, string userId,
		PickingTask pickingTask, int locationId, string snapShot, Pallet palletSource, DateOnly? bestBefore)
		{
			var now = _dateTimeProvider.UtcNow;
			//var bestBefore = palletSource.ProductsOnPallet.Single().BestBefore;
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issueId);
			if (oldPallet is null)
			{
				var newNumberPallet = await _palletRepo.GetNextPalletIdAsync();
				var newPickingPallet = Pallet.CreatePickingPallet(newNumberPallet, locationId, now, productId, quantity, bestBefore);
				var palletId = _palletRepo.AddPallet(newPickingPallet);
				newPickingPallet.ReserveToIssue(issueId, userId, snapShot);
				pickingTask.CompleteTask(newPickingPallet, palletSource, quantity, userId, now);
				return new CreateNewPickingPalletResult(true, palletId, newNumberPallet);
			}
			else
			{
				oldPallet.AddOrIncreaseProductQuantity(productId, quantity, now, bestBefore);
				pickingTask.CompleteTask(oldPallet, palletSource, quantity, userId, now);
				return new CreateNewPickingPalletResult(false, oldPallet.Id, oldPallet.PalletNumber);
			}
		}
	}

}
