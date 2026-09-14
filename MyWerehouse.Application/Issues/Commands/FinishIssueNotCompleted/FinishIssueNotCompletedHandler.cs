using System;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted
{
	public class FinishIssueNotCompletedHandler(IUnitOfWork unitOfWork,
		IIssueRepo issueRepo) : IRequestHandler<FinishIssueNotCompletedCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<Unit>> Handle(FinishIssueNotCompletedCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId, ct);
			if (issue == null)
				return AppResult<Unit>.Fail("Issue was not found.");
			var palletsReturn = issue.RemoveNotLoadedPallets(request.UserId);
			issue.FinishIssueNotCompleted(request.UserId);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Issue {request.IssueId} was closed.");
		}
	}
}
