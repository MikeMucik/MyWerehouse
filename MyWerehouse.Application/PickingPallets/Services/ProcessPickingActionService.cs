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
			//Utwórz nową lub dodaj do starej
			var pickingPallet = await CreatePalletOrAddToPallet(issue.Id, productId,
				quantityToPick, userId, bestBefore, pickingTask, pickingCompletion, rampNumber, sourcePallet);

			//Usuwanie towaru z palety źródłowej -> do nowej metody pomocnicznej
			productOnSourcePallet.DecreaseQuantity(quantityToPick);
			if (productOnSourcePallet.Quantity == 0)//archiwizuj jeśli pusta
			{
				sourcePallet.ChangeStatus(PalletStatus.Archived);
			}
			//Historia zawsze
			sourcePallet.AddHistory(ReasonMovement.Picking, userId, sourcePallet.Location.ToSnapshot());
			return ProcessPickingActionResult.Ok(pickingPallet.PalletId);
		}
		private async Task<CreatePalletResult> CreatePalletOrAddToPallet(Guid issueId, Guid productId,
			int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask,
			PickingCompletion pickingCompletion, int rampNumber, Pallet palletSource)
		{
			// Pobierz aktywną paletę pickingową (zamknięte palety nie są zwracane)
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(pickingTask.IssueId);
			if (oldPallet == null)//Tworzę nową paletę	
			{
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = bestBefore;
				var pallet = Pallet.Create(newIdPallet, rampNumber);
				pallet.ChangeStatus(PalletStatus.Picking);//Bo paleta kompletacyjna
				var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
				var snapShot = location.ToSnapshot();
				pallet.AddProduct(productId, quantity, sourcePalletBB);
				var palletId = _palletRepo.AddPallet(pallet);
				pallet.ReserveToIssue(issueId, userId, snapShot);
				//Obsługa pickingTask
				MarkPickingTask(pickingTask, pickingCompletion, pallet, palletSource, userId, quantity);								
				return new CreatePalletResult(true, palletId); //pokaż komunikat weź nową paletę
			}
			else//dodaje do już istniejącej
			{
				oldPallet.AddOrIncreaseProductQuantity(productId, quantity, bestBefore);
				//Obsługa pickingTask	
				MarkPickingTask(pickingTask, pickingCompletion, oldPallet, palletSource, userId, quantity);
				return new CreatePalletResult(false, oldPallet.Id);
			}
		}
		private static void MarkPickingTask(PickingTask pickingTask, PickingCompletion pickingCompletion, Pallet pickingPallet,
			Pallet palletSource, string userId, int quantity)
		{
			if (pickingCompletion == PickingCompletion.Full)
			{
				pickingTask.MarkPicked(pickingPallet.Id, pickingPallet.PalletNumber, palletSource.Id, palletSource.PalletNumber, userId); //
			}
			else
			{
				pickingTask.MarkPartiallyPicked(pickingPallet.Id, pickingPallet.PalletNumber, palletSource.Id, palletSource.PalletNumber, quantity, userId);
			}
			pickingPallet.AddHistory(ReasonMovement.Picking, userId, pickingPallet.Location.ToSnapshot());
		}
	}

}
