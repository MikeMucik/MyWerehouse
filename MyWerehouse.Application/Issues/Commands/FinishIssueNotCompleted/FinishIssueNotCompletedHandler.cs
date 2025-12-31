using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Commands.ChangeQuantity;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted
{
	public class FinishIssueNotCompletedHandler : IRequestHandler<FinishIssueNotCompletedCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		public FinishIssueNotCompletedHandler(WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
		}
		public async Task<IssueResult> Handle(FinishIssueNotCompletedCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new IssueException(request.IssueId);
				var palletsReturn =	issue.RemoveNotLoadedPallets();
				//var palletsReturn = new List<Pallet>();

				//var toReturn = issue.Pallets
				//	.Where(p => p.Status != PalletStatus.Loaded)
				//	.ToList();
				//foreach (var pallet in toReturn)
				//{					
				//		pallet.Status = PalletStatus.Available;
				//		pallet.IssueId = null;
				//		issue.Pallets.Remove(pallet);
				//		palletsReturn.Add(pallet);					
				//}
				issue.IssueStatus = IssueStatus.IsShipped;
				issue.PerformedBy = request.UserId;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				foreach (var pallet in issue.Pallets)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.ToLoad,
							request.UserId,
							PalletStatus.Loaded,
							null), ct);
					foreach (var product in pallet.ProductsOnPallet)
					{
						await _mediator.Send(new ChangeQuantityCommand(product.ProductId, -product.Quantity), ct);
					}
				}
				foreach (var pallet in palletsReturn)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.Correction,
							request.UserId,
							PalletStatus.Available,
							null
						), ct);
				}
				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);
				
				return IssueResult.Ok($"Zamknięto wydanie {request.IssueId}.");
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
				throw new InvalidOperationException("Wystąpił błąd podczas zatwierdzania niekompletnego zlecenia.", ex);
			}
		}
	}
}


