using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted
{
	public class FinishIssueNotCompletedHandler : IRequestHandler<FinishIssueNotCompletedCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		public FinishIssueNotCompletedHandler(WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo)
		{
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
		}
		public async Task<AppResult<Unit>> Handle(FinishIssueNotCompletedCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);

			var palletsReturn = issue.RemoveNotLoadedPallets(request.UserId);
			issue.FinishIssueNotCompleted(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Zamknięto wydanie {request.IssueId}.");
		}
	}
}