using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading
{
	public class VerifyIssueAfterLoadingHandler(WerehouseDbContext dbContext,
		IIssueRepo issueRepo) : IRequestHandler<VerifyIssueAfterLoadingCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _dbContext = dbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<Unit>> Handle(VerifyIssueAfterLoadingCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			issue.VeryfiedAfterLoading(request.VerifiedBy);
			await _dbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Załadunek zatwierdzony, zasoby uaktulanione.");
		}
	}
}
