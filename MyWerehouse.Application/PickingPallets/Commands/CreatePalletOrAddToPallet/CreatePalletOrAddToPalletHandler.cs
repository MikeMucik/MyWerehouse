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

namespace MyWerehouse.Application.PickingPallets.Commands.CreatePalletOrAddToPallet
{
	public class CreatePalletOrAddToPalletHandler(IPalletRepo palletRepo,
		IEventCollector eventCollector) : IRequestHandler<CreatePalletOrAddToPalletCommand, Unit>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IEventCollector _eventCollector = eventCollector;

		public async Task<Unit> Handle(CreatePalletOrAddToPalletCommand request, CancellationToken ct)
		{
			
			var oldPallet = await _palletRepo.GetPickingPalletByIssueId(request.IssueId);
			if (oldPallet == null)
			{
				//pokaż komunikat weź nową paletę
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
				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id,
				pallet.LocationId,
				ReasonMovement.Picking,
				request.UserId,
				PalletStatus.Picking,
				null));
			}
			else
			{
				var pickingPallet = oldPallet;
				var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId ==request.ProductId);
				if (existingProduct != null)
				{
					existingProduct.Quantity +=request.Quantity;
				}
				else
				{
					pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
					{
						ProductId =request.ProductId,
						Quantity =request.Quantity,
						DateAdded = DateTime.UtcNow,
					});
				}
				_eventCollector.Add(new CreatePalletOperationNotification(oldPallet.Id,
				oldPallet.LocationId,
				ReasonMovement.Picking,
				request.UserId,
				PalletStatus.Picking,
				null));
			}
			return Unit.Value;
		}
	}
}
