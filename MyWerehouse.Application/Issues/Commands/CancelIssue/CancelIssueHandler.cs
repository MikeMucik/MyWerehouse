using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Commands;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CancelIssue
{
	public class CancelIssueHandler : IRequestHandler<CancelIssueCommand, IssueResult>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;		
		private readonly ICreateReversePickingService _createReversePickingService;
		public CancelIssueHandler(IIssueRepo issueRepo,
			IPickingTaskRepo pickingTaskRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator,
			IEventCollector eventCollector,			
			ICreateReversePickingService createReversePickingService
			)
		{
			_issueRepo = issueRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
			_eventCollector = eventCollector;			
			_createReversePickingService = createReversePickingService;
		}
		public async Task<IssueResult> Handle(CancelIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new NotFoundIssueException(request.IssueId);
				var listPallet = new List<Pallet>();
				//anulowanie zlecenia dla pełnych palet
				foreach (var pallet in issue.Pallets)
				{
					if (pallet.ReceiptId != null)//paleta kompletacyjna nie ma ReceiptId tylko  palety z przyjęcia
					{
						pallet.Status = PalletStatus.Available;
						pallet.IssueId = null;
						listPallet.Add(pallet);
					}
				}
				//palety kompletacyjne i zadania pickingu 
				var restPallets = issue.Pallets.Except(listPallet).ToList();
				foreach (var p in restPallets)
				{
					await _createReversePickingService.CreateReversePicking(p.Id, request.UserId); ;
				}
				//usuń alokacje/pickingTask jeśli nie zrobione				
				var virtualPallets = await _pickingTaskRepo.GetVirtualPalletsByIssue(request.IssueId);
				foreach (var vp in virtualPallets)
				{
					var pickingTaskToRemove = vp.PickingTasks
						.Where(a => a.PickingStatus == PickingStatus.Allocated && a.IssueId == issue.Id)
						.ToList();
					foreach (var pickingTask in pickingTaskToRemove)
					{
						pickingTask.PickingStatus = PickingStatus.Cancelled;

						_eventCollector.Add(new CreateHistoryPickingNotification(new HistoryDataPicking
							(
								pickingTask.Id,
								pickingTask.VirtualPallet.PalletId,
								pickingTask.IssueId,
								pickingTask.ProductId,
								pickingTask.RequestedQuantity,
								0,
								PickingStatus.Allocated,
								pickingTask.PickingStatus,
								request.UserId,
								DateTime.UtcNow)));
						vp.PickingTasks.Remove(pickingTask);
						_pickingTaskRepo.DeletePickingTask(pickingTask);
					}
					//usuń virtualPallet jeśli należy tylko do tego zlecenia
					if (vp.PickingTasks.Count == 0)
					{
						_pickingPalletRepo.DeleteVirtualPalletPicking(vp);
						vp.Pallet.Status = PalletStatus.Available;
					}
				}
				issue.IssueStatus = IssueStatus.Cancelled;
				issue.PerformedBy = request.UserId;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);

				foreach (var p in listPallet)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(p.Id, 1, ReasonMovement.CancelIssue, request.UserId, PalletStatus.Available, null), ct);
				}
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, ct);
				}				
				return IssueResult.Ok($"Anulowano zlecenie {request.IssueId}.");
			}
			catch (NotFoundIssueException ie)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ie.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
//var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(request.IssueId);
//var pickingTasksNotDone = pickingTasks.Where(a=>a.PickingStatus == PickingStatus.Allocated).ToList();
//foreach (var pickingTask in pickingTasksNotDone)
//{
//	pickingTask.PickingStatus = PickingStatus.Cancelled;
//	_eventCollector.Add(new CreateHistoryPickingNotification(pickingTask.VirtualPalletId,
//		pickingTask.Id,
//		request.UserId,
//		PickingStatus.Allocated,
//		pickingTask.Quantity));
//	_pickingTaskRepo.DeletePickingTask(pickingTask);//czy wszystkie
//}