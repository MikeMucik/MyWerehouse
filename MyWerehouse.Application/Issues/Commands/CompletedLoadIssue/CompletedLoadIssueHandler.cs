using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Commands.CompletedLoadIssue
{
	public class CompletedLoadIssueHandler(IUnitOfWork unitOfWork,
		IIssueRepo issueRepo) : IRequestHandler<CompletedLoadIssueCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;	
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<Unit>> Handle(CompletedLoadIssueCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId, ct);
			if (issue == null)
				return AppResult<Unit>.Fail("Issue was not found.");
			issue.CompletedLoad(request.UserId);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Loading completed for issue {request.IssueId}.");
		}
	}
}
