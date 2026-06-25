using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.Services
{
	public class ProcessPickingActionService(IPalletRepo palletRepo,
		ILocationRepo locationRepo, IProductRepo productRepo) : IProcessPickingActionService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<ProcessPickingActionResult> ProcessPicking(Pallet sourcePallet, Issue issue, Guid productId,
			int quantityToPick, string userId, PickingTask pickingTask, PickingCompletion pickingCompletion, int rampNumber)
		{
			var productOnSourcePallet = sourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == productId);
			if (productOnSourcePallet is null)
				return ProcessPickingActionResult.Fail($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			var bestBefore = pickingTask.BestBefore;
			//Utwórz nową lub dodaj do starej
			var pickingPallet = await CreateNewPalletOrAddToOldPickingPallet(issue.Id, productId,
				quantityToPick, userId, bestBefore, pickingTask, pickingCompletion, rampNumber, sourcePallet);
			var product = await _productRepo.GetProductByIdAsync(productId);
			//Usuwanie towaru z palety źródłowej -> do nowej metody pomocnicznej
			productOnSourcePallet.DecreaseQuantity(quantityToPick);
			if (productOnSourcePallet.Quantity == 0)//archiwizuj jeśli pusta
			{
				sourcePallet.ChangeStatus(PalletStatus.Archived);
			}
			if (pickingPallet.NewPalletCreated)
			{
				sourcePallet.AddHistory(ReasonForPallet.Picking, userId, sourcePallet.Location.ToSnapshot());
				return ProcessPickingActionResult.OkWithNewPallet(pickingPallet.PalletId, pickingPallet.PalletNumber,
					$"Weź nową paletę dla zlecenia. Towar: {product.SKU} ilość:{quantityToPick}");
			}
			else
			{
				sourcePallet.AddHistory(ReasonForPallet.Picking, userId, sourcePallet.Location.ToSnapshot());
				return ProcessPickingActionResult.Ok(pickingPallet.PalletId, pickingPallet.PalletNumber,
					$"Dołącz towar do starej palety kompletacyjnej. Towar: {product.SKU} ilość:{quantityToPick}");
			}
		}
		private async Task<CreateNewPickingPalletResult> CreateNewPalletOrAddToOldPickingPallet(Guid issueId, Guid productId,
			int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask,
			PickingCompletion pickingCompletion, int rampNumber, Pallet palletSource)
		{
			// Pobierz aktywną paletę pickingową (zamknięte palety nie są zwracane)
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(pickingTask.IssueId);
			if (oldPallet == null)//Tworzę nową paletę	
			{
				var newNumberPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = bestBefore;
				var pallet = Pallet.Create(newNumberPallet, rampNumber);
				pallet.ChangeStatus(PalletStatus.Picking);//Bo paleta kompletacyjna
				var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
				var snapShot = location.ToSnapshot();
				pallet.AddProduct(productId, quantity, sourcePalletBB);
				var palletId = _palletRepo.AddPallet(pallet);
				pallet.ReserveToIssue(issueId, userId, snapShot);
				//Obsługa pickingTask
				MarkPickingTask(pickingTask, pickingCompletion, pallet, palletSource, userId, quantity);
				return new CreateNewPickingPalletResult(true, palletId, newNumberPallet); //pokaż komunikat weź nową paletę
			}
			else//dodaje do już istniejącej
			{
				oldPallet.AddOrIncreaseProductQuantity(productId, quantity, bestBefore);
				//Obsługa pickingTask	
				MarkPickingTask(pickingTask, pickingCompletion, oldPallet, palletSource, userId, quantity);
				return new CreateNewPickingPalletResult(false, oldPallet.Id, oldPallet.PalletNumber);
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
			pickingPallet.AddHistory(ReasonForPallet.Picking, userId, pickingPallet.Location.ToSnapshot());
		}
	}

}
