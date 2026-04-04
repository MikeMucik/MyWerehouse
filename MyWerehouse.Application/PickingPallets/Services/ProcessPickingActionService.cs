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
		public async Task<ProcessPickingActionResult> ProcessPicking(Pallet sourcePallet, Issue issue, Guid productId,
			int quantityToPick, string userId, PickingTask pickingTask, PickingCompletion pickingCompletion, int rampNumber)
		{
			var productOnSourcePallet = sourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == productId);
			if (productOnSourcePallet is null)
				return ProcessPickingActionResult.Fail($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			var bestBefore = pickingTask.BestBefore;
			
			var pickingPallet =	await _createPalletOrAddToPalletService.CreatePalletOrAddToPallet(issue, productId,
				quantityToPick, userId, bestBefore, pickingTask, pickingCompletion, rampNumber);
			//Usuwanie towaru z palety źródłowej
			productOnSourcePallet.AddQuantity(-quantityToPick);
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
		
	}
}
