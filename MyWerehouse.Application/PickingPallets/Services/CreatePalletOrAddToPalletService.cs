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

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class CreatePalletOrAddToPalletService : ICreatePalletOrAddToPalletService
	{
		private readonly IPalletRepo _palletRepo;
		public CreatePalletOrAddToPalletService(IPalletRepo palletRepo)
		{
			_palletRepo = palletRepo;		
		}
		public async Task<CreatePalletResult> CreatePalletOrAddToPallet(Issue issue, int productId, int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask, PickingCompletion pickingCompletion)
		{
			//var issue = await _issueRepo.GetIssueByIdAsync(issueId);			
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issue.Id);
			if (oldPallet == null)
			{
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = bestBefore;
				var pallet = new Pallet
				{
					Id = newIdPallet,
					Status = PalletStatus.Picking,
					IssueId = issue.Id,
					LocationId = 100100,//pickingzone location
					DateReceived = DateTime.UtcNow,
					ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet
							{
								PalletId = newIdPallet,
								ProductId =productId,
								Quantity =quantity,
								DateAdded = DateTime.UtcNow,
								BestBefore = sourcePalletBB
							}
						},
				};
				_palletRepo.AddPallet(pallet);
				//issue.Pallets.Add(pallet);
				issue.AssignPallet(pallet, userId);
				pallet.AddHistory(PalletStatus.Picking, ReasonMovement.Picking, userId);
				if (pickingCompletion == PickingCompletion.Full)
				{
					pickingTask.MarkPicked(newIdPallet); //
				}
				else
				{
					pickingTask.MarkPartiallyPicked(newIdPallet, quantity);
				}				
				return new CreatePalletResult(true, newIdPallet); //pokaż komunikat weź nową paletę
			}
			else
			{
				var pickingPallet = oldPallet;
				var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
				if (existingProduct != null)
				{
					existingProduct.Quantity += quantity;
				}
				else
				{
					pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
					{
						ProductId = productId,
						Quantity = quantity,
						DateAdded = DateTime.UtcNow,
					});
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
