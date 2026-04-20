using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class ProcessPickingActionService(IPalletRepo palletRepo,
		ILocationRepo locationRepo) : IProcessPickingActionService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<ProcessPickingActionResult> ProcessPicking(Pallet sourcePallet, Issue issue, Guid productId,
			int quantityToPick, string userId, PickingTask pickingTask, PickingCompletion pickingCompletion, int rampNumber)
		{
			var productOnSourcePallet = sourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == productId);
			if (productOnSourcePallet is null)
				return ProcessPickingActionResult.Fail($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			var bestBefore = pickingTask.BestBefore;
			var pickingPallet = await CreatePalletOrAddToPallet(issue, productId,
				quantityToPick, userId, bestBefore, pickingTask, pickingCompletion, rampNumber);
			//Usuwanie towaru z palety źródłowej
			productOnSourcePallet.DecreaseQuantity(quantityToPick);
			if (productOnSourcePallet.Quantity == 0)
			{				
				sourcePallet.AddHistory(ReasonMovement.Picking, userId, sourcePallet.Location.ToSnopShot());
				sourcePallet.ChangeStatus(PalletStatus.Archived);
			}
			else
			{
				sourcePallet.AddHistory(ReasonMovement.Picking, userId, sourcePallet.Location.ToSnopShot());
			}
			return ProcessPickingActionResult.Ok(pickingPallet.PalletId);
		}
		private async Task<CreatePalletResult> CreatePalletOrAddToPallet(Issue issue, Guid productId,
			int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask,
			PickingCompletion pickingCompletion, int rampNumber)
		{
			//Tworzę nową paletę		
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issue.Id);
			if (oldPallet == null)
			{
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = bestBefore;
				var pallet = Pallet.Create(newIdPallet, rampNumber);
				pallet.ChangeStatus(PalletStatus.Picking);//Bo paleta kompletacyjna
				var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
				var snapShot = location.ToSnopShot();
				pallet.AddProduct(productId, quantity, sourcePalletBB);
				var palletId = _palletRepo.AddPallet(pallet);
				issue.ReservePallet(pallet, userId);//
				pallet.ReserveToIssue(issue, userId, snapShot);
				if (pickingCompletion == PickingCompletion.Full)
				{
					pickingTask.MarkPicked(palletId); //
				}
				else
				{
					pickingTask.MarkPartiallyPicked(palletId, quantity);
				}
				return new CreatePalletResult(true, palletId); //pokaż komunikat weź nową paletę
			}
			else
			{
				var pickingPallet = oldPallet;
				pickingPallet.AddOrIncreaseProductQuantity(productId, quantity,bestBefore);				
				if (pickingCompletion == PickingCompletion.Full)
				{
					pickingTask.MarkPicked(oldPallet.Id); //
				}
				else
				{
					pickingTask.MarkPartiallyPicked(oldPallet.Id, quantity);
				}
				oldPallet.AddHistory(ReasonMovement.Picking, userId, oldPallet.Location.ToSnopShot());
				return new CreatePalletResult(false, oldPallet.Id);
			}
		}
	}

}
