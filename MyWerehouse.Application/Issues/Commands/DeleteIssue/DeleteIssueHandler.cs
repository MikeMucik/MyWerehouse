using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.DeleteIssue
{
	public class DeleteIssueHandler(IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<DeleteIssueCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<Unit>> Handle(DeleteIssueCommand request, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var issueToDelete = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issueToDelete == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
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
					return AppResult<Unit>.Fail($"Zlecenia {issueToDelete.Id} nie można anulować.", ErrorType.Conflict);
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Usunięto zamówienie o numerze {issueToDelete.Id}.");
		}
	}
}