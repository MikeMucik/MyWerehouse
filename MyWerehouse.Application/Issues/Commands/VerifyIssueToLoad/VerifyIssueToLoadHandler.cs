using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public class VerifyIssueToLoadHandler(IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<VerifyIssueToLoadCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<Unit>> Handle(VerifyIssueToLoadCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			//TODO check requested amount = prepered amount
			issue.ConfirmToLoad(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Wydanie zatwierdzono.");
		}
	}
}
