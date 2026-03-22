using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class ProcessPickingActionService : IProcessPickingActionService
	{
		private readonly ICreatePalletOrAddToPalletService _createPalletOrAddToPalletService;

		public ProcessPickingActionService(
			ICreatePalletOrAddToPalletService createPalletOrAddToPalletService)
		{
			_createPalletOrAddToPalletService = createPalletOrAddToPalletService;			
		}
		public async Task<ProcessPickingActionResult> ProcessPicking(Pallet sourcePallet, Issue issue, Guid productId, int quantityToPick, string userId, PickingTask pickingTask, PickingCompletion pickingCompletion)
		{
			var productOnSourcePallet = sourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == productId);
			if (productOnSourcePallet is null)
				return ProcessPickingActionResult.Fail($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			var bestBefore = pickingTask.BestBefore;
			
			var pickingPallet =	await _createPalletOrAddToPalletService.CreatePalletOrAddToPallet(issue, productId,
				quantityToPick, userId, bestBefore, pickingTask, pickingCompletion);
			//Usuwanie towaru z palety źródłowej
			productOnSourcePallet.Quantity -= quantityToPick;
			if (productOnSourcePallet.Quantity == 0)
			{
				sourcePallet.AddHistory(PalletStatus.Archived, ReasonMovement.Picking, userId);
			}
			else
			{
				sourcePallet.AddHistory(PalletStatus.ToPicking, ReasonMovement.Picking, userId);
			}
			return ProcessPickingActionResult.Ok(pickingPallet.PalletId);
		}



		//private async void CreatePalletOrAddToPallet(int issueId, int productId, int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask, PickingCompletion pickingCompletion)
		//{
		//	var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issueId);
		//	if (oldPallet == null)
		//	{
		//		var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
		//		var sourcePalletBB = bestBefore;
		//		var pallet = new Pallet
		//		{
		//			Id = newIdPallet,
		//			Status = PalletStatus.Picking,
		//			IssueId = issueId,
		//			LocationId = 100100,//pickingzone location
		//			DateReceived = DateTime.UtcNow,
		//			ProductsOnPallet = new List<ProductOnPallet>
		//				{
		//					new ProductOnPallet
		//					{
		//						PalletId = newIdPallet,
		//						ProductId =productId,
		//						Quantity =quantity,
		//						DateAdded = DateTime.UtcNow,
		//						BestBefore = sourcePalletBB
		//					}
		//				},
		//		};
		//		_palletRepo.AddPallet(pallet);
		//		if (pickingCompletion == PickingCompletion.Full)
		//		{
		//			pickingTask.MarkPicked(newIdPallet); //
		//		}
		//		else
		//		{
		//			pickingTask.MarkPartiallyPicked(newIdPallet, quantity);
		//		}

		//		_eventCollector.Add(new ChangeStatusOfPalletNotification(pallet.Id, pallet.LocationId,
		//		ReasonMovement.Picking, userId, PalletStatus.Picking, null));
		//		return new CreatePalletResult(true, newIdPallet); //pokaż komunikat weź nową paletę
		//	}
		//	else
		//	{
		//		var pickingPallet = oldPallet;
		//		var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
		//		if (existingProduct != null)
		//		{
		//			existingProduct.Quantity += quantity;
		//		}
		//		else
		//		{
		//			pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
		//			{
		//				ProductId = productId,
		//				Quantity = quantity,
		//				DateAdded = DateTime.UtcNow,
		//			});
		//		}
		//		if (pickingCompletion == PickingCompletion.Full)
		//		{
		//			pickingTask.MarkPicked(oldPallet.Id); //
		//		}
		//		else
		//		{
		//			pickingTask.MarkPartiallyPicked(oldPallet.Id, quantity);
		//		}
		//		_eventCollector.Add(new ChangeStatusOfPalletNotification(oldPallet.Id, oldPallet.LocationId,
		//		ReasonMovement.Picking, userId, PalletStatus.Picking, null));
		//		return new CreatePalletResult(false, oldPallet.Id);
		//	}
		//}
	}
}
