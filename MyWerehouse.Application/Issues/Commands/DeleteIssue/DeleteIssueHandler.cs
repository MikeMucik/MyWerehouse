using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Events;
using MyWerehouse.Domain.Picking.Models;
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
					?? throw new NotFoundIssueException(request.IssueId);

				List<INotification> palletList = [];
				List<INotification> pickingTaskList = [];
				switch (issueToDelete.IssueStatus)
				{
					case IssueStatus.New:
						_issueRepo.DeleteIssue(issueToDelete);
						break;

					case IssueStatus.Pending:
					case IssueStatus.NotComplete:
						issueToDelete.IssueStatus = IssueStatus.Cancelled;
						var listPalletsToRemove = issueToDelete.Pallets.ToList();
						foreach (var pallet in listPalletsToRemove)
						{
							issueToDelete.DetachPallet(pallet, request.UserId);
						}
						foreach (var pickingTask in issueToDelete.PickingTasks)
						{
							pickingTask.PickingStatus = PickingStatus.Cancelled;
							pickingTask.RequestedQuantity = 0;							
							pickingTaskList.Add(new CreateHistoryPickingNotification(
								pickingTask.Id,
								//pickingTask.PickingTaskNumber,
								pickingTask.VirtualPallet.PalletId,
								pickingTask.IssueId,
								pickingTask.IssueNumber,
								pickingTask.ProductId,
								pickingTask.RequestedQuantity,
								0,
								PickingStatus.Allocated,
								PickingStatus.Cancelled,
								request.UserId,
								DateTime.UtcNow));							
						}
						break;
					default:
						throw new NotFoundIssueException($"Zlecenia {issueToDelete.Id} nie można anulować.");
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				if (!(issueToDelete.IssueStatus == IssueStatus.New))
				{
					issueToDelete.AddHistory(request.UserId);
				}
				foreach (var p in palletList)
				{
					await _mediator.Publish(p, ct);
				}
				foreach (var a in pickingTaskList)
				{
					await _mediator.Publish(a, ct);
				}
				return IssueResult.Ok($"Usunięto zamówienie o numerze {issueToDelete.Id}.");
			}
			catch (NotFoundIssueException ei)
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
