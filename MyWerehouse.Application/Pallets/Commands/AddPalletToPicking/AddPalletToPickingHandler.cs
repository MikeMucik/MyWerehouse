using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct;
using MyWerehouse.Application.Pallets.Queries.GetOneAvailablePalletByProduct;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Pallets.Commands.AddPalletToPicking
{
	public class AddPalletToPickingHandler : IRequestHandler<AddPalletToPickingCommand, AddPalletToPickingResult>
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
		public async Task<AddPalletToPickingResult> Handle(AddPalletToPickingCommand request, CancellationToken ct)
		{
			var newPallet = request.Pallet;
			var newVirtualPicking = new VirtualPallet
			{
				Pallet = newPallet,
				PalletId = newPallet.Id,
				DateMoved = DateTime.UtcNow,
				LocationId = newPallet.LocationId,				
				InitialPalletQuantity = newPallet.ProductsOnPallet.FirstOrDefault(p => p.PalletId == newPallet.Id).Quantity ,//zakładam że jest jeden towar
				PickingTasks = new List<PickingTask>()
			};
			var virtualPallet = _pickingPalletRepo.AddPalletToPicking(newVirtualPicking);			
			_palletRepo.ChangePalletStatus(newPallet.Id, PalletStatus.ToPicking); //zmiana statusu dla palety
			_eventCollector.Add(new CreatePalletOperationNotification(newPallet.Id,	newPallet.LocationId,
				ReasonMovement.Picking,	request.UserId,	PalletStatus.ToPicking,	null));
			return AddPalletToPickingResult.Ok(virtualPallet );				
		}
	}
}
