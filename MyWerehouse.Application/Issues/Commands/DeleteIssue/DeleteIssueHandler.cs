using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.DeleteIssue
{
	public class DeleteIssueHandler : IRequestHandler<DeleteIssueCommand, IssueResult>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		public DeleteIssueHandler(IIssueRepo issueRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator)
		{
			_issueRepo = issueRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;
		}
		public async Task<IssueResult> Handle(DeleteIssueCommand request, CancellationToken ct)
		{

			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issueToDelete = await _issueRepo.GetIssueByIdAsync(request.IssueId)
					?? throw new IssueException(request.IssueId);

				List<INotification> palletList = [];
				List<INotification> allocationList = [];
				switch (issueToDelete.IssueStatus)
				{
					case IssueStatus.New:
						_issueRepo.DeleteIssue(issueToDelete);
						break;

					case IssueStatus.Pending:
					case IssueStatus.NotComplete:
						issueToDelete.IssueStatus = IssueStatus.Cancelled;
						foreach (var pallet in issueToDelete.Pallets)
						{
							pallet.IssueId = null;
							pallet.Status = PalletStatus.Available;
							palletList.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId, ReasonMovement.Correction, request.UserId, PalletStatus.Available, null));
						}
						foreach (var allocation in issueToDelete.Allocations)
						{
							allocation.PickingStatus = PickingStatus.Cancelled;
							allocation.Quantity = 0;
							var historyPicking = new HistoryDataPicking
							(
								allocation.Id,
								allocation.VirtualPallet.PalletId,
								allocation.IssueId,
									 allocation.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId,
									 allocation.Quantity,
									 0,
									 PickingStatus.Allocated,
									 PickingStatus.Cancelled,
									 request.UserId,
									 DateTime.UtcNow
								);
							allocationList.Add(new CreateHistoryPickingNotification(historyPicking));
							//allocationList.Add(new CreateHistoryPickingNotification(allocation.VirtualPalletId, allocation.Id, request.UserId, PickingStatus.Allocated, 0));

						}
						break;
					default:
						throw new IssueException($"Zlecenia {issueToDelete.Id} nie można anulować.");
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				if (!(issueToDelete.IssueStatus == IssueStatus.New))
				{
					await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);
				}
				foreach (var p in palletList)
				{
					await _mediator.Publish(p, ct);
				}
				foreach (var a in allocationList)
				{
					await _mediator.Publish(a, ct);
				}
				return IssueResult.Ok($"Usunięto zamówienie o numerze {issueToDelete.Id}.");
			}
			catch (IssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			}
		}
	}
}
