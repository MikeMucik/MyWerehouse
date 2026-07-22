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

namespace MyWerehouse.Application.Picking.Services
{
	public class ProcessPickingActionService(IPalletRepo palletRepo,
		ILocationRepo locationRepo, IProductRepo productRepo, IDateTimeProvider dateTimeProvider) : IProcessPickingActionService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<ProcessPickingActionResult> ExecuteProcessPicking(Pallet sourcePallet, Issue issue, Guid productId,
			int quantityToPick, string userId, PickingTask pickingTask, PickingCompletion pickingCompletion, int locationId)
		{
			var location = await _locationRepo.GetLocationByIdAsync(locationId);
			if (location == null) return ProcessPickingActionResult.Fail("Nie znaleziono rampy.");
			var snapshotPickingPallet = location.ToSnapshot();
			var now = _dateTimeProvider.UtcNow;
			var productOnSourcePallet = sourcePallet.GetProductOnPallet(productId);
			if (productOnSourcePallet is null)
				return ProcessPickingActionResult.Fail($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			var pickingPallet = await GetOrCreatePickingPallet(issue.Id, productId, quantityToPick, userId,
				pickingTask, locationId, snapshotPickingPallet, sourcePallet, pickingCompletion);
			//var bestBefore = pickingTask.BestBefore;
			var productSKU = await _productRepo.GetSKUForProductAsync(productId);
			var snapshotSourcePallet = sourcePallet.Location.ToSnapshot();			
			sourcePallet.PickProduct(productOnSourcePallet, quantityToPick, userId, snapshotSourcePallet);
			if (pickingPallet.NewPalletCreated)
			{
				return ProcessPickingActionResult.OkWithNewPallet(pickingPallet.PalletId, pickingPallet.PalletNumber,
					$"Weź nową paletę dla zlecenia. Towar: {productSKU} ilość:{quantityToPick}");
			}
			else
			{
				return ProcessPickingActionResult.Ok(pickingPallet.PalletId, pickingPallet.PalletNumber,
					$"Dołącz towar do starej palety kompletacyjnej. Towar: {productSKU} ilość:{quantityToPick}");
			}
		}

		public async Task<CreateNewPickingPalletResult> GetOrCreatePickingPallet(Guid issueId, Guid productId, int quantity, string userId,
		PickingTask pickingTask, int locationId, string snapShot, Pallet palletSource, PickingCompletion completion)
		{
			var now = _dateTimeProvider.UtcNow;
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issueId);
			if (oldPallet is null)
			{
				var newNumberPallet = await _palletRepo.GetNextPalletIdAsync();
				var pallet = Pallet.CreatePickingPallet(newNumberPallet, locationId, now, productId, quantity, pickingTask.BestBefore);
				var palletId = _palletRepo.AddPallet(pallet);
				pallet.ReserveToIssue(issueId, userId, snapShot);
				CompleteTask(pickingTask, completion, pallet, palletSource, userId, quantity, now);
				return new CreateNewPickingPalletResult(true, palletId, newNumberPallet);
			}
			else
			{
				oldPallet.AddOrIncreaseProductQuantity(productId, quantity, now, pickingTask.BestBefore);
				CompleteTask(pickingTask, completion, oldPallet, palletSource, userId, quantity, now);
				return new CreateNewPickingPalletResult(false, oldPallet.Id, oldPallet.PalletNumber);
			}
		}

		private static void CompleteTask(PickingTask pickingTask, PickingCompletion pickingCompletion, Pallet pickingPallet,
			Pallet palletSource, string userId, int quantity, DateTime now)
		{
			if (pickingCompletion == PickingCompletion.Full)
			{
				pickingTask.MarkPicked(pickingPallet.Id, pickingPallet.PalletNumber, palletSource.Id, palletSource.PalletNumber, userId, now);
			}
			else
			{
				pickingTask.MarkPartiallyPicked(pickingPallet.Id, pickingPallet.PalletNumber, palletSource.Id, palletSource.PalletNumber, quantity, userId, now);
			}
			pickingPallet.AddHistory(ReasonForPallet.Picking, userId, pickingPallet.Location.ToSnapshot());
		}
	}

}
