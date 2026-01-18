using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.CreatePalletOrAddToPallet
{
	public class CreatePalletOrAddToPalletHandler(IPalletRepo palletRepo,
		IEventCollector eventCollector) : IRequestHandler<CreatePalletOrAddToPalletCommand, CreatePalletResult>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IEventCollector _eventCollector = eventCollector;

		public async Task<CreatePalletResult> Handle(CreatePalletOrAddToPalletCommand request, CancellationToken ct)
		{

			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(request.IssueId);
			if (oldPallet == null)
			{
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = request.BestBefore;
				var pallet = new Pallet
				{
					Id = newIdPallet,
					Status = PalletStatus.Picking,
					IssueId = request.IssueId,
					LocationId = 100100,//lokalizacja że polu pickingu
					DateReceived = DateTime.UtcNow,
					ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet
							{
								PalletId = newIdPallet,
								ProductId =request.ProductId,
								Quantity =request.Quantity,
								DateAdded = DateTime.UtcNow,
								BestBefore = sourcePalletBB
							}
						},
				};
				_palletRepo.AddPallet(pallet);
				if(request.PickingCompletion == PickingCompletion.Full)
				{
					request.Allocation.MarkPicked(newIdPallet); //
				}
				else
				{
					request.Allocation.MarkPartiallyPicked(newIdPallet, request.Quantity);
				}
				
				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
				ReasonMovement.Picking, request.UserId, PalletStatus.Picking, null));
				return new CreatePalletResult(true, newIdPallet); //pokaż komunikat weź nową paletę
			}
			else
			{
				var pickingPallet = oldPallet;
				var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == request.ProductId);
				if (existingProduct != null)
				{
					existingProduct.Quantity += request.Quantity;
				}
				else
				{
					pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
					{
						ProductId = request.ProductId,
						Quantity = request.Quantity,
						DateAdded = DateTime.UtcNow,
					});
				}
				if (request.PickingCompletion == PickingCompletion.Full)
				{
					request.Allocation.MarkPicked(oldPallet.Id); //
				}
				else
				{
					request.Allocation.MarkPartiallyPicked(oldPallet.Id, request.Quantity);
				}
				_eventCollector.Add(new CreatePalletOperationNotification(oldPallet.Id, oldPallet.LocationId,
				ReasonMovement.Picking, request.UserId, PalletStatus.Picking, null));
				return new CreatePalletResult(false, oldPallet.Id);
			}
		}
	}
}
