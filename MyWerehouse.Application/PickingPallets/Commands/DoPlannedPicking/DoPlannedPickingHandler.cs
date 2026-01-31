using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.DoPicking
{
	public class DoPlannedPickingHandler : IRequestHandler<DoPlannedPickingCommand, PickingResult>
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IIssueRepo _issueRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService;
		public DoPlannedPickingHandler(IPickingTaskRepo pickingTaskRepo,
			IPalletRepo palletRepo,
			IIssueRepo issueRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator,
			IEventCollector eventCollector,
			IAddPickingTaskToIssueService addPickingTaskToIssueService,
			IProcessPickingActionService processPickingActionService)
		{
			_pickingTaskRepo = pickingTaskRepo;
			_palletRepo = palletRepo;
			_issueRepo = issueRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
			_eventCollector = eventCollector;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
			_processPickingActionService = processPickingActionService;
		}
		public async Task<PickingResult> Handle(DoPlannedPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{				
				var newPickingTask = new PickingTask();
				var pickingTaskToChange = await _pickingTaskRepo.GetPickingTaskAsync(request.PickingTaskDTO.Id);
				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(pickingTaskToChange.VirtualPalletId);
				var issueId = pickingTaskToChange.IssueId;
				var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new NotFoundIssueException(issueId);
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.PickingTaskDTO.SourcePalletId)
					?? throw new NotFoundPalletException(request.PickingTaskDTO.SourcePalletId);
				if (issue.IssueStatus == IssueStatus.Pending) { issue.IssueStatus = IssueStatus.InProgress; }
				var neededQuantity = request.PickingTaskDTO.RequestedQuantity;
				var pickedQuantity = request.PickingTaskDTO.PickedQuantity;
				var completion = PickingCompletion.Full;
				if (pickedQuantity <= 0 || pickedQuantity > neededQuantity)
				{
					await transaction.RollbackAsync(ct);
					return PickingResult.Fail("Operacja nie dozwolona");
				}
				if (neededQuantity > pickedQuantity)
				{
					completion = PickingCompletion.Partial;
				}

				await _processPickingActionService.ProcessPicking(sourcePallet, issue, request.PickingTaskDTO.ProductId,
						request.PickingTaskDTO.PickedQuantity, request.UserId, pickingTaskToChange, completion);
				var historyPicking = new HistoryDataPicking(pickingTaskToChange.Id, pickingTaskToChange.VirtualPallet.PalletId,
							pickingTaskToChange.IssueId, pickingTaskToChange.ProductId, pickingTaskToChange.RequestedQuantity,
							request.PickingTaskDTO.PickedQuantity, PickingStatus.Allocated, pickingTaskToChange.PickingStatus,
							request.UserId, DateTime.UtcNow);
				_eventCollector.Add(new CreateHistoryPickingNotification(historyPicking));

				if (neededQuantity == pickedQuantity)
				{
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					foreach (var evn in _eventCollector.Events)
					{
						await _mediator.Publish(evn, CancellationToken.None);
					}
					return PickingResult.Ok("Towar dołączono do zlecenia");
				}
				else
				{
					//pallet lock with non-conformity 
					sourcePallet.Status = PalletStatus.OnHold;
					_eventCollector.Add(new CreatePalletOperationNotification(sourcePallet.Id, sourcePallet.LocationId,
						ReasonMovement.Correction, request.UserId, PalletStatus.OnHold, null));
					var newQuantityToPickingTask = neededQuantity - pickedQuantity;
					var newVirtualPallet =await _addPickingTaskToIssueService.AddPickingTaskToIssue(null, new List<VirtualPallet>(),
						issue, pickingTaskToChange.ProductId, newQuantityToPickingTask, pickingTaskToChange.BestBefore, request.UserId);
					
					if (newVirtualPallet.Success == false)
					{
						await transaction.RollbackAsync(ct);
						return PickingResult.Fail(newVirtualPallet.Message);
					}

					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					foreach (var evn in _eventCollector.Events)
					{
						await _mediator.Publish(evn, CancellationToken.None);
					}
					foreach (var factory in _eventCollector.DeferredEvents)
					{
						await _mediator.Publish( factory(), CancellationToken.None);//await
					}
					return PickingResult.Ok("Towar dołączono do zlecenia, wykonano nie pełne zadanie kompletacyjne, stworzono dodatkowe zadanie do pickingu. Poproś o nowe palety do kompletacji.");
				}				
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
