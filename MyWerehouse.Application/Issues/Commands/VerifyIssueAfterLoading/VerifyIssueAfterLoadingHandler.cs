using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading
{
	public class VerifyIssueAfterLoadingHandler(WerehouseDbContext dbContext,
		IIssueRepo issueRepo) : IRequestHandler<VerifyIssueAfterLoadingCommand, IssueResult>
	{		
		private readonly WerehouseDbContext _dbContext = dbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<IssueResult> Handle(VerifyIssueAfterLoadingCommand request, CancellationToken ct)
		{
			using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new NotFoundIssueException(request.IssueId);
				
				issue.VeryfiedAfterLoading(request.VerifiedBy);
				await _dbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				
				return IssueResult.Ok("Załadunek zatwierdzony, zasoby uaktulanione.");
			}
			catch (NotFoundIssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			catch(DomainException exx)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(exx.Message);
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
