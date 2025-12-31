using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading
{
	public class VerifyIssueAfterLoadingHandler(IMediator mediator,
		WerehouseDbContext dbContext,
		IIssueRepo issueRepo,
		IInventoryService inventoryService
		) : IRequestHandler<VerifyIssueAfterLoadingCommand, IssueResult>
	{
		private readonly IMediator _mediator = mediator;
		private readonly WerehouseDbContext _dbContext = dbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IInventoryService _inventoryService = inventoryService;

		public async Task<IssueResult> Handle(VerifyIssueAfterLoadingCommand request, CancellationToken ct)
		{
			using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new IssueException(request.IssueId);
				if (issue.Pallets.Any(p => p.Status != PalletStatus.Loaded))
					throw new IssueException("Nie wszystkie palety mają status Loaded.");

				if (issue.IssueStatus != IssueStatus.IsShipped) throw new IssueException("Nie zakończono załadunku.");
				issue.IssueStatus = IssueStatus.Archived;
				issue.PerformedBy = request.VerifiedBy;
				List<INotification> notificationList = [];
				foreach (var pallet in issue.Pallets)
				{
					pallet.Status = PalletStatus.Archived;
					foreach (var product in pallet.ProductsOnPallet)
					{
						await _inventoryService.ChangeProductQuantityAsync(product.ProductId, -product.Quantity);
					}
					notificationList.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId, ReasonMovement.Loaded, request.VerifiedBy, PalletStatus.Archived, null));
				}
				await _dbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, request.VerifiedBy), ct);
				foreach (var p in notificationList)
				{
					await _mediator.Publish(p, ct);
				}
				return IssueResult.Ok("Załadunek zatwierdzony, zasoby uaktulanione.");
			}
			catch (IssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			catch (InventoryException einv)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(einv.Message);
			}
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
