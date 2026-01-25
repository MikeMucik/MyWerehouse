using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteManualPicking
{
	public class ExecuteManualPickingHandler : IRequestHandler<ExecuteManualPickingCommand, PickingResult>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService;
		public ExecuteManualPickingHandler(IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator,
			IEventCollector eventCollector,
			IAddPickingTaskToIssueService addPickingTaskToIssueService,
			IProcessPickingActionService processPickingActionService)
		{
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
			_eventCollector = eventCollector;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
			_processPickingActionService = processPickingActionService;
		}
		public async Task<PickingResult> Handle(ExecuteManualPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId)
					?? throw new NotFoundPalletException(request.PalletId);
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
					?? throw new NotFoundIssueException(request.IssueId);
				var product = pallet.ProductsOnPallet.FirstOrDefault()
					?? throw new InvalidOperationException($"Paleta {request.PalletId} jest pusta.");

				// Oblicz, ile faktycznie można/trzeba skompletować
				var pickingTasksForIssue = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(request.IssueId, product.ProductId);
				var neededQuantity = pickingTasksForIssue.Where(a => a.PickingStatus == PickingStatus.Allocated).Sum(a => a.Quantity);
				var quantityToPick = Math.Min(neededQuantity, product.Quantity);

				if (quantityToPick <= 0)
				{
					return PickingResult.Fail("Brak zapotrzebowania na ten produkt dla wybranego zlecenia.");
				}

				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletId));
				//var vp = 
				var listOfVirtualPallets = new List<VirtualPallet> { virtualPallet };
				if (virtualPallet == null || virtualPallet.Id == 0)
				{
					virtualPallet = new VirtualPallet
					{
						Pallet = pallet,
						PalletId = pallet.Id,
						DateMoved = DateTime.UtcNow,
						LocationId = pallet.LocationId,
						InitialPalletQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
						PickingTasks = new List<PickingTask>()						
						// Dodaj inne wymagane pola (np. Status, CreatedAt = DateTime.UtcNow)
					};
					_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
				}
				//await ReducePickingTaskAsync(issue, product.ProductId, quantityToPick, userId);
				//var pickingTaskList = await _mediator.Send(new ReducePickingTaskCommand(issue, product.ProductId, quantityToPick, request.UserId), ct);

				var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issue.Id, product.ProductId);
				foreach (var item in pickingTasks)
				{
					item.PickingStatus = PickingStatus.Cancelled;
					_eventCollector.Add(new CreateHistoryPickingNotification(new HistoryDataPicking
					(
									item.Id,
									item.VirtualPallet.PalletId,
									item.IssueId,
									item.ProductId,
									item.Quantity,
									0,
									PickingStatus.Allocated,
									item.PickingStatus,
									request.UserId,
									DateTime.UtcNow
					)));
				}

				//await ProcessPickingActionAsync(pallet, issue, product.ProductId, quantityToPick, userId);
				//await _mediator.Send(new ProcessPickingActionCommand(pallet, issue, product.ProductId, quantityToPick, request.UserId, pickingTaskList.First(), PickingCompletion.Full), ct);
				// Ta logika jest specyficzna dla manuala (tworzenie nowej alokacji) już nie TODO
				var newPickingTask = await _addPickingTaskToIssueService.AddPickingTaskToIssue(null,
					listOfVirtualPallets, issue, product.ProductId, quantityToPick, product.BestBefore, request.UserId);
				await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, quantityToPick, request.UserId, newPickingTask.PickingTask.First(), PickingCompletion.Full);
				//await _mediator.Send(new ProcessPickingActionCommand(pallet, issue, product.ProductId, quantityToPick, request.UserId, newPickingTask.PickingTask.First(), PickingCompletion.Full), ct);

				if (neededQuantity > quantityToPick)
				{
					var newPickingTaskNext = await _addPickingTaskToIssueService.AddPickingTaskToIssue(null,
					listOfVirtualPallets, issue, product.ProductId, quantityToPick, product.BestBefore, request.UserId);
				}
				//	PickingTaskUtilis.CreatePickingTask(virtualPallet, issue, quantityToPick);
				//_pickingTaskRepo.AddPickingTask(newPickingTask);
				//var historyPicking = new HistoryDataPicking(newPickingTask.Id,
				//				newPickingTask.VirtualPallet.PalletId,
				//				newPickingTask.IssueId,
				//					 newPickingTask.ProductId,
				//					 newPickingTask.Quantity,
				//					 0,
				//					 PickingStatus.Available,
				//					 newPickingTask.PickingStatus,
				//				request.UserId,
				//					 DateTime.UtcNow
				//				);
				//_eventCollector.AddDeferred( () =>
				//		new CreateHistoryPickingNotification(
				//			new HistoryDataPicking
				//			(
				//				newPickingTask.Id,
				//				newPickingTask.VirtualPallet.PalletId,
				//				newPickingTask.IssueId,
				//					 newPickingTask.ProductId,
				//					 newPickingTask.Quantity,
				//					 0,
				//					 PickingStatus.Available,
				//					 newPickingTask.PickingStatus,
				//					request.UserId,
				//					 DateTime.UtcNow
				//				)));

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
