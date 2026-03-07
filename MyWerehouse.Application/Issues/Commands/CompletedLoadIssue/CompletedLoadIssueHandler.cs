using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CompletedLoadIssue
{
	public class CompletedLoadIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo) : IRequestHandler<CompletedLoadIssueCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<Unit>> Handle(CompletedLoadIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if (issue == null)
					return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
				issue.CompletedLoad(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, $"Zakończono załadunek {request.IssueId}.");
			}
			catch (DomainException exd)
			{
				await transaction.RollbackAsync(ct);
				return AppResult<Unit>.Fail(exd.Message);
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
