using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Commands.CreatePalletOrAddToPallet;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.ProcessPickingAction
{
	public class ProcessPickingActionHandler : IRequestHandler<ProcessPickingActionCommand, ProcessPickingActionResult>
	{
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		private readonly IAllocationRepo _allocationRepo;
		public ProcessPickingActionHandler(
			IMediator mediator,
			IEventCollector eventCollector,
			IAllocationRepo allocationRepo)
		{
			_mediator = mediator;
			_eventCollector = eventCollector;
			_allocationRepo = allocationRepo;
		}
		public async Task<ProcessPickingActionResult> Handle(ProcessPickingActionCommand request, CancellationToken ct)
		{
			var productOnSourcePallet = request.SourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == request.ProductId);
			//var allocationDoing = await _allocationRepo.GetAllocationAsync(request.)
			if (productOnSourcePallet is null)
				return ProcessPickingActionResult.Fail($"Na palecie {request.SourcePallet.Id} nie znaleziono produktu o Id : {request.ProductId}.");
				//?? throw new PalletException($"Na palecie {request.SourcePallet.Id} nie znaleziono produktu o Id : {request.ProductId}.");
			var bestBofore = productOnSourcePallet.BestBefore;
			await _mediator.Send(new CreatePalletOrAddToPalletCommand(request.Issue.Id, request.ProductId,
				request.QuantityToPick, request.UserId, bestBofore, request.Allocation, request.PickingCompletion), ct);
			productOnSourcePallet.Quantity -= request.QuantityToPick;
			if (productOnSourcePallet.Quantity == 0)
			{
				request.SourcePallet.Status = PalletStatus.Archived;
				_eventCollector.Add(new CreatePalletOperationNotification(request.SourcePallet.Id, request.SourcePallet.LocationId,
				ReasonMovement.Picking,	request.Issue.PerformedBy, PalletStatus.Archived, null));
			}
			else
			{
				_eventCollector.Add(new CreatePalletOperationNotification(request.SourcePallet.Id, request.SourcePallet.LocationId,
				ReasonMovement.Picking,	request.Issue.PerformedBy, PalletStatus.ToPicking, null));
			}
			return ProcessPickingActionResult.Ok();
		}
	}
}
