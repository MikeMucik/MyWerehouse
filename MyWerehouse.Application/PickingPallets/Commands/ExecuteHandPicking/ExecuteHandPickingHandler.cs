using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking
{
	public class ExecuteHandPickingHandler : IRequestHandler<ExecuteHandPickingCommand, PickingResult>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService;
		private readonly IHandPickingTaskRepo _handPickingTaskRepo;

		public ExecuteHandPickingHandler(IPalletRepo palletRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator,
			IEventCollector eventCollector,
			IAddPickingTaskToIssueService addPickingTaskToIssueService,
			IProcessPickingActionService processPickingActionService,
			IHandPickingTaskRepo handPickingTaskRepo)
		{
			_palletRepo = palletRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
			_eventCollector = eventCollector;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
			_processPickingActionService = processPickingActionService;
			_handPickingTaskRepo = handPickingTaskRepo;
		}
		public async Task<PickingResult> Handle(ExecuteHandPickingCommand command, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletId)
					?? throw new NotFoundPalletException(command.PalletId);
				//if (_werehouseDbContext.Entry(pallet).State == EntityState.Detached)
				//{
				//	_werehouseDbContext.Attach(pallet);
				//}
				if (pallet.ProductsOnPallet.Count > 1)
				{
					return PickingResult.Fail("Zadania nie można zrealizować, paleta nie nadaje się do pobrań.");
				}
				var issue = await _issueRepo.GetIssueByIdAsync(command.IssueId)
					?? throw new NotFoundIssueException(command.IssueId);
				var product = pallet.ProductsOnPallet.FirstOrDefault()
					?? throw new InvalidOperationException($"Paleta {command.PalletId} jest pusta.");
				
				if (command.Quanitity > pallet.ProductsOnPallet.First().Quantity)
				{
					return PickingResult.Fail("Zadania nie można zrealizować, mniej na palecie niż chęć pobrania");
				}
				
				var pickingHand = await _handPickingTaskRepo.GetByIssueAndProductAsync(command.IssueId, product.Id);
				if (pickingHand == null)
				{
					return PickingResult.Fail("Brak zapotrzebowania na ten asortyment."); 
				}
				if (command.Quanitity > (pickingHand.Quantity - pickingHand.PickedQuantity))
				{
					return PickingResult.Fail("Chcesz pobrać więcej niż potrzeba.");
				}
					if (pickingHand.PickingStatus == PickingStatus.Picked)
				{
					return PickingResult.Fail("Zapotrzebowania na ten asortyment już zrealizowane");
				}
				VirtualPallet virtualPallet;
				var vpId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(command.PalletId);
				if (vpId != 0)
				{
					int virtualPalletId = vpId;
					virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(virtualPalletId);
				}
				else
				{
					virtualPallet = new VirtualPallet
					{
						//Pallet = pallet,
						PalletId = pallet.Id,
						DateMoved = DateTime.UtcNow,
						LocationId = pallet.LocationId,
						InitialPalletQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
						PickingTasks = new List<PickingTask>()
					};
					_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
				}
				if (command.Quanitity > virtualPallet.RemainingQuantity)
				{
					return PickingResult.Fail("Zadania nie można zrealizować, mniej na palecie niż chęć pobrania");
				}

				var newPickingTaskInfo = await _addPickingTaskToIssueService.AddOnePickingTaskToIssue(virtualPallet, issue, product.ProductId, command.Quanitity, product.BestBefore, command.UserId);
				var newPickingTask = newPickingTaskInfo.OnePickingTask;
				await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, command.Quanitity, command.UserId, newPickingTask, PickingCompletion.Full);

				if (pickingHand.Quantity == command.Quanitity)
				{
					pickingHand.MarkHandPicked();
				}
				else
				{
					pickingHand.MarkHandPartiallyPicked(command.Quanitity);
				}

				await _werehouseDbContext.SaveChangesAsync(ct);
				
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, ct);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(factory(), ct);
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);

				return PickingResult.Ok("Towar dołączono do zlecenia");

			}
			catch (NotFoundPalletException pnfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(pnfEx.Message);
			}
			catch (NotFoundIssueException onfEx)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(onfEx.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}

}
