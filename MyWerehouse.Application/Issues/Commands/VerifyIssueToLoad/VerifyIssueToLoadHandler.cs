using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public class VerifyIssueToLoadHandler : IRequestHandler<VerifyIssueToLoadCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public VerifyIssueToLoadHandler(IIssueRepo issueRepo,
			WerehouseDbContext werehouseDbContext)
		{
			_issueRepo = issueRepo;
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task<AppResult<Unit>> Handle(VerifyIssueToLoadCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if (issue == null)
					return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
				//TODO check requested amount = prepered amount
				issue.ConfirmToLoad(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Wydanie zatwierdzono.");
			}
			catch (DomainException exx)
			{
				await transaction.RollbackAsync(ct);
				return AppResult<Unit>.Fail(exx.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas zatwierdzania zlecenia.", ex);
			}
		}
	}
}
