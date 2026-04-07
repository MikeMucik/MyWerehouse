using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class CreatePalletOrAddToPalletService : ICreatePalletOrAddToPalletService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly ILocationRepo _locationRepo;
		public CreatePalletOrAddToPalletService(IPalletRepo palletRepo, ILocationRepo locationRepo)
		{
			_palletRepo = palletRepo;
			_locationRepo = locationRepo;
		}
		public async Task<CreatePalletResult> CreatePalletOrAddToPallet(Issue issue, Guid productId,
			int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask,
			PickingCompletion pickingCompletion, int rampNumber)
		{
			//var issue = await _issueRepo.GetIssueByIdAsync(issueId);	
			//Tworzę nową paletę		
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issue.Id);
			if (oldPallet == null)
			{
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = bestBefore;
				var pallet = Pallet.Create(newIdPallet, rampNumber);
				pallet.ChangeStatus(PalletStatus.Picking);//Bo paleta kompletacyjna
				var location = await _locationRepo.GetLocationByIdAsync(rampNumber);
				//if (location == null)
				//{
				//	return Task<CreatePalletResult>.Fail($"Brak lokalizacji o numerze {rampNumber}.", ErrorType.NotFound);
				//}
				//pallet.AddLocation(location);
				var snapShot = location.ToSnopShot();
				pallet.AddProduct(productId, quantity, sourcePalletBB);
				
				var palletId =	_palletRepo.AddPallet(pallet);
				issue.ReservePallet(pallet, userId);//
				pallet.ReserveToIssue(issue, userId);
				//pallet.AddHistory(PalletStatus.Picking, ReasonMovement.Picking, userId);
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
				var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
				if (existingProduct != null)
				{
					existingProduct.AddQuantity(quantity);
				}
				else
				{
					pickingPallet.AddProduct(productId, quantity, bestBefore);
					//pickingPallet.ProductsOnPallet.Add(ProductOnPallet.Create(productId,pickingPallet.Id, quantity, DateTime.UtcNow, bestBefore));
				}
				if (pickingCompletion == PickingCompletion.Full)
				{
					pickingTask.MarkPicked(oldPallet.Id); //
				}
				else
				{
					pickingTask.MarkPartiallyPicked(oldPallet.Id, quantity);
				}
				oldPallet.AddHistory(PalletStatus.Picking, ReasonMovement.Picking, userId);
				
				return new CreatePalletResult(false, oldPallet.Id);
			}
		}
	}
}
