using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
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
						?? throw new NotFoundIssueException(request.IssueId);
				var palletsReturn =	issue.RemoveNotLoadedPallets(request.UserId);
				
				issue.IssueStatus = IssueStatus.IsShipped;
				issue.PerformedBy = request.UserId;
				// ten fragment chyba do wywalenia
				var listPallets = issue.Pallets.ToList();
				foreach (var pallet in listPallets)
				{
					pallet.ChangeStatus(PalletStatus.Loaded, ReasonMovement.ToLoad, request.UserId);
				}
				

				await _mediator.Publish(new AddHistoryForIssueNotification(request.IssueId, request.UserId), ct);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return IssueResult.Ok($"Zamknięto wydanie {request.IssueId}.");
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
				throw new InvalidOperationException("Wystąpił błąd podczas zatwierdzania niekompletnego zlecenia.", ex);
			}
		}
	}
}


