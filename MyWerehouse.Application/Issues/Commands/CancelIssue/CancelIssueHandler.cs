using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Commands;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.ReversePickings.Command.CreateTaskToReversePicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CancelIssue
{
	public class CancelIssueHandler : IRequestHandler<CancelIssueCommand, IssueResult>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		private readonly IEventCollector _eventCollector;
		private readonly ICommandCollector _commandCollector;
		public CancelIssueHandler(IIssueRepo issueRepo,
			IAllocationRepo allocationRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator,
			IEventCollector eventCollector
			, ICommandCollector commandCollector
			)
		{
			_issueRepo = issueRepo;
			_allocationRepo = allocationRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
			_eventCollector = eventCollector;
			_commandCollector = commandCollector;
		}
		public async Task<IssueResult> Handle(CancelIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new IssueException(request.IssueId);
				var listPallet = new List<Pallet>();
				//anulowanie zlecenia dla pełnych palet
				foreach (var pallet in issue.Pallets)
				{
					if (pallet.ReceiptId != null)//paleta kompletacyjna nie ma ReceiptId tylko pełne palety z przyjęcia
					{
						pallet.Status = PalletStatus.Available;
						pallet.IssueId = null;
						pallet.LocationId = 1;//lokalizacja rampy
						listPallet.Add(pallet);
					}
				}
				//palety kompletacyjne i zadania pickingu 
				var restPallets = issue.Pallets.Except(listPallet).ToList();
				foreach (var p in restPallets)
				{
					_commandCollector.Add(new CreateTaskToReversePickingCommand(p.Id, request.UserId));
				}
				//usuń alokacje jeśli nie zrobione
				//usuń virtualPallet jeśli należy tylko do tego zlecenia
				var virtualPallets = await _allocationRepo.GetVirtualPalletsByIssue(request.IssueId);
				foreach (var vp in virtualPallets)
				{
					var allocationToRemove = vp.Allocations
						.Where(a => a.PickingStatus == PickingStatus.Allocated && a.IssueId == issue.Id)
						.ToList();
					foreach (var allocation in allocationToRemove)
					{
						allocation.PickingStatus = PickingStatus.Cancelled;

						_eventCollector.Add(new CreateHistoryPickingNotification(new HistoryDataPicking
							(
								allocation.Id,
								allocation.VirtualPallet.PalletId,
								allocation.IssueId,
									 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 allocation.Quantity,
									 0,
									 PickingStatus.Allocated,
									 allocation.PickingStatus,
									 request.UserId,
									 DateTime.UtcNow
								)));
						vp.Allocations.Remove(allocation);
						//_werehouseDbContext.Allocations.Remove(allocation);
						_allocationRepo.DeleteAllocation(allocation);
					}
					if (vp.Allocations.Count == 0)//warunek bez sensu bo nie było zapisu
					{
						_pickingPalletRepo.DeleteVirtualPalletPicking(vp);//zadanie po commit? czy w DBSet usuwa wszytko>
						vp.Pallet.Status = PalletStatus.Available;
					}
				}
				issue.IssueStatus = IssueStatus.Cancelled;
				issue.PerformedBy = request.UserId;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);
				foreach (var req in _commandCollector.Requests)
				{
					await _mediator.Send(req, ct);
				}
				foreach (var p in listPallet)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(p.Id, 1, ReasonMovement.CancelIssue, request.UserId, PalletStatus.Available, null), ct);
				}
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, ct);
				}
				//_eventCollector.Clear();
				return IssueResult.Ok($"Anulowano zlecenie {request.IssueId}.");
			}
			catch (IssueException ie)
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
//var allocations = await _allocationRepo.GetAllocationsByIssueIdAsync(request.IssueId);
//var allocationsNotDone = allocations.Where(a=>a.PickingStatus == PickingStatus.Allocated).ToList();
//foreach (var allocation in allocationsNotDone)
//{
//	allocation.PickingStatus = PickingStatus.Cancelled;
//	_eventCollector.Add(new CreateHistoryPickingNotification(allocation.VirtualPalletId,
//		allocation.Id,
//		request.UserId,
//		PickingStatus.Allocated,
//		allocation.Quantity));
//	_allocationRepo.DeleteAllocation(allocation);//czy wszystkie
//}