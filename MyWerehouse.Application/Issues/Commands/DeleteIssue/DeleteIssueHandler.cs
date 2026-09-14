using System;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Commands.DeleteIssue
{
	public class DeleteIssueHandler(IIssueRepo issueRepo,
		IUnitOfWork unitOfWork,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<DeleteIssueCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<Unit>> Handle(DeleteIssueCommand request, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var issueToDelete = await _issueRepo.GetIssueByIdAsync(request.IssueId, ct);
			if (issueToDelete == null)
				return AppResult<Unit>.Fail("Issue was not found.");
			switch (issueToDelete.IssueStatus)
			{
				case IssueStatus.New:
					_issueRepo.DeleteIssue(issueToDelete);
					break;
				case IssueStatus.Pending:
				case IssueStatus.RequiresCorrection:
					issueToDelete.CancelIssue(request.UserId, now);
					break;
				default:
					return AppResult<Unit>.Fail($"Issue {issueToDelete.Id} cannot be cancelled.", ErrorType.Conflict);
			}
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Issue {issueToDelete.Id} was deleted.");
		}
	}
}
