using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Commands.ConfirmIssueAfterLoading
{
	public class ConfirmIssueAfterLoadingHandler(IUnitOfWork unitOfWork,
		IIssueRepo issueRepo) : IRequestHandler<ConfirmIssueAfterLoadingCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;	
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<Unit>> Handle(ConfirmIssueAfterLoadingCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId, ct);
			if (issue == null)
				return AppResult<Unit>.Fail("Issue was not found.");
			issue.ConfirmAfterLoading(request.ConfirmedBy);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Loading confirmed and inventory updated.");
		}
	}
}
