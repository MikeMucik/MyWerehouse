using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading
{
	public class VerifyIssueAfterLoadingHandler(IMediator mediator,
		WerehouseDbContext dbContext,
		IIssueRepo issueRepo) : IRequestHandler<VerifyIssueAfterLoadingCommand, IssueResult>
	{
		private readonly IMediator _mediator = mediator;
		private readonly WerehouseDbContext _dbContext = dbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<IssueResult> Handle(VerifyIssueAfterLoadingCommand request, CancellationToken ct)
		{
			using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new NotFoundIssueException(request.IssueId);
				if (issue.Pallets.Any(p => p.Status != PalletStatus.Loaded))
					throw new NotFoundIssueException("Nie wszystkie palety mają status Loaded.");

				if (issue.IssueStatus != IssueStatus.IsShipped) throw new NotFoundIssueException("Nie zakończono załadunku.");
				issue.IssueStatus = IssueStatus.Archived;
				issue.PerformedBy = request.VerifiedBy;
				
				var stockChanges = issue.Pallets
					.SelectMany(p => p.ProductsOnPallet)
					.GroupBy(p => p.ProductId)
					.Select(g => new StockItemChange(g.Key, g.Sum(x => x.Quantity)))
					.ToList();
				var notifications = new List<INotification>();

				foreach (var pallet in issue.Pallets)
				{
					pallet.ChangeStatus(PalletStatus.Archived, ReasonMovement.Loaded, request.VerifiedBy);					
				}
				if (stockChanges.Count != 0)
				{
					notifications.Add(
						new ChangeStockNotification(
							StockChangeType.Decrease,
							stockChanges
						)
					);
				}
				await _dbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new AddHistoryForIssueNotification(issue.Id, request.VerifiedBy), ct);				
				foreach (var notification in notifications)
				{
					await _mediator.Publish(notification, ct);
				}

				return IssueResult.Ok("Załadunek zatwierdzony, zasoby uaktulanione.");
			}
			catch (NotFoundIssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			//catch (InventoryException einv)
			//{
			//	await transaction.RollbackAsync(ct);
			//	return IssueResult.Fail(einv.Message);
			//}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return IssueResult.Fail("Wystąpił nieoczenikawy błąd przy weryfikacji");
			}
		}
	}

}
