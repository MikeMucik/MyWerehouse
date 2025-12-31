using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CompletedIssue
{
	public class CompletedIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IMediator mediator) : IRequestHandler<CompletedIssueCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMediator _mediator = mediator;

		public async Task<IssueResult> Handle(CompletedIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId) ?? throw new IssueException(request.IssueId);
				foreach (var pallet in issue.Pallets)
				{
					if (pallet.Status != PalletStatus.Loaded)
					{
						throw new IssueException("Nie załadowano wszystkich palet.");
					}
				}
				issue.IssueStatus = IssueStatus.IsShipped;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);
				return IssueResult.Ok($"Zakończono załadunek {request.IssueId}.");
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
				throw new InvalidOperationException("Wystąpił błąd podczas zatwierdzania załadunku zlecenia.", ex);
			}
		}
	}
}
