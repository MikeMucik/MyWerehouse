using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CompletedLoadIssue
{
	public class CompletedLoadIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo) : IRequestHandler<CompletedLoadIssueCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<IssueResult> Handle(CompletedLoadIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId) 
					?? throw new NotFoundIssueException(request.IssueId);
				
				issue.CompletedLoad(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return IssueResult.Ok($"Zakończono załadunek {request.IssueId}.");
			}
			catch (NotFoundIssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			catch(DomainException exd)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(exd.Message);
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
