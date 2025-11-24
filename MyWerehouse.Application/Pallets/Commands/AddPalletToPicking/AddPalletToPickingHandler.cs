using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct;
using MyWerehouse.Application.Pallets.Queries.GetOneAvailablePalletByProduct;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Pallets.Commands.AddPalletToPicking
{
	public class AddPalletToPickingHandler : IRequestHandler<AddPalletToPickingCommand, VirtualPallet>
	{
		private readonly IMediator _mediator;
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IEventCollector _eventCollector;
		public AddPalletToPickingHandler(IMediator mediator,
			IPalletRepo palletRepo,
			IEventCollector eventCollector,
			IPickingPalletRepo pickingPalletRepo)
		{
			_mediator = mediator;
			_palletRepo = palletRepo;
			_eventCollector = eventCollector;
			_pickingPalletRepo = pickingPalletRepo;
		}
		public async Task<VirtualPallet> Handle(AddPalletToPickingCommand request, CancellationToken ct)
		{
			var newPallet = new Pallet();
			if (request.Pallets == null)
			{
				 newPallet = await _mediator.Send(new GetOneAvailablePalletByProductQuery(request.ProductId, request.BestBefore), ct);					
			}
			else newPallet = request.Pallets.FirstOrDefault();
			if (newPallet == null) { throw new PalletNotFoundException("Brak palet do pickingu"); }
			if (newPallet.ProductsOnPallet == null) { throw new PalletNotFoundException("Brak towaru na palecie do pickingu"); }
			var newVirtualPicking = new VirtualPallet
			{
				Pallet = newPallet,
				PalletId = newPallet.Id,
				DateMoved = DateTime.UtcNow,
				LocationId = newPallet.LocationId,				
				IssueInitialQuantity = newPallet.ProductsOnPallet.FirstOrDefault(p => p.PalletId == newPallet.Id).Quantity ,//zakładam że jest jeden towar
				Allocations = new List<Allocation>()
			};
			var virtualPallet = _pickingPalletRepo.AddPalletToPicking(newVirtualPicking);
			request.Pallets?.Remove(newPallet);
			_palletRepo.ChangePalletStatus(newPallet.Id, PalletStatus.ToPicking); //zmiana statusu dla palety
			_eventCollector.Add(new CreatePalletOperationNotification(newPallet.Id,
				newPallet.LocationId,
				ReasonMovement.Picking,
				request.UserId,
				PalletStatus.ToPicking,
				null));
			return virtualPallet;
		}
	}
}
