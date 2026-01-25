using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class CreatePalletOrAddToPalletService : ICreatePalletOrAddToPalletService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IEventCollector _eventCollector;
		public CreatePalletOrAddToPalletService(IPalletRepo palletRepo, IEventCollector eventCollector)
		{
			_palletRepo = palletRepo;
			_eventCollector = eventCollector;
		}
		public async Task<CreatePalletResult> CreatePalletOrAddToPallet(int issueId, int productId, int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask, PickingCompletion pickingCompletion)
		{
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(issueId);
			if (oldPallet == null)
			{
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = bestBefore;
				var pallet = new Pallet
				{
					Id = newIdPallet,
					Status = PalletStatus.Picking,
					IssueId = issueId,
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
				if (pickingCompletion == PickingCompletion.Full)
				{
					pickingTask.MarkPicked(newIdPallet); //
				}
				else
				{
					pickingTask.MarkPartiallyPicked(newIdPallet, quantity);
				}
				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
				ReasonMovement.Picking, userId, PalletStatus.Picking, null));
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
				_eventCollector.Add(new CreatePalletOperationNotification(oldPallet.Id, oldPallet.LocationId,
				ReasonMovement.Picking, userId, PalletStatus.Picking, null));
				return new CreatePalletResult(false, oldPallet.Id);
			}
		}
	}
}
