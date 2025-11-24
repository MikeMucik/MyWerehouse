using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Commands.Issue.VerifyIssueAfterLoading
{
	//public class VerifyIssueAfterLoadingHandler(IMediator mediator,
	//	WerehouseDbContext dbContext,
	//	IIssueRepo issueRepo,
	//	IInventoryService inventoryService,
	//	IHistoryService historyService) :
	//	IRequestHandler<VerifyIssueAfterLoadingCommand, IssueResult>
	//{
	//	private readonly IMediator _mediator = mediator;
	//	private readonly WerehouseDbContext _dbContext = dbContext;
	//	private readonly IIssueRepo _issueRepo = issueRepo;
	//	private readonly IInventoryService _inventoryService = inventoryService;
	//	private readonly IHistoryService _historyService = historyService;

	//	public async Task<IssueResult> Handle(VerifyIssueAfterLoadingCommand request,  CancellationToken cancellationToken)
	//	{
	//		using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
	//		try
	//		{
	//			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
	//					?? throw new IssueNotFoundException(request.IssueId);
	//			issue.PerformedBy = request.VerifiedBy;
	//			if (issue.IssueStatus != IssueStatus.IsShipped) throw new IssueNotFoundException("Nie zakończono załadunku.");
	//			issue.IssueStatus = IssueStatus.Archived;
	//			foreach (var pallet in issue.Pallets)
	//			{
	//				pallet.Status = PalletStatus.Archived;
	//				foreach (var product in pallet.ProductsOnPallet)
	//				{
	//					await _inventoryService.ChangeProductQuantityAsync(product.ProductId, -product.Quantity);						
	//				}
	//				_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Loaded, request.VerifiedBy, PalletStatus.Archived, null);
	//			}
	//			//_historyService.CreateHistoryIssue(issue);
	//			await _dbContext.SaveChangesAsync(cancellationToken);
	//			await transaction.CommitAsync(cancellationToken);
	//			await _mediator.Send(new CreateHistoryIssueCommand(issue.Id, issue.PerformedBy), cancellationToken);

	//			return IssueResult.Ok("Załadunek zatwierdzony, zasoby uaktulanione.");
	//		}
	//		catch (IssueNotFoundException ei)
	//		{
	//			await transaction.RollbackAsync(cancellationToken);
	//			return IssueResult.Fail(ei.Message);
	//		}
	//		catch (InventoryException einv)
	//		{
	//			await transaction.RollbackAsync(cancellationToken);
	//			return IssueResult.Fail(einv.Message);
	//		}
	//		catch (Exception ex)
	//		{
	//			await transaction.RollbackAsync(cancellationToken);
	//			// Loguj ex dla developera!
	//			//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
	//			return IssueResult.Fail("Wystąpił nieoczenikawy błąd przy weryfikacji");
	//		}
	//	}
	//}
 
}
