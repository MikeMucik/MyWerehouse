using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.DeleteIssue
{
	public class DeleteIssueHandler(IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<DeleteIssueCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<Unit>> Handle(DeleteIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			var issueToDelete = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issueToDelete == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			switch (issueToDelete.IssueStatus)
			{
				case IssueStatus.New:
					_issueRepo.DeleteIssue(issueToDelete);
					break;
				case IssueStatus.Pending:
				case IssueStatus.NotComplete:
					issueToDelete.CancelIssue(request.UserId);
					break;
				default:
					return AppResult<Unit>.Fail($"Zlecenia {issueToDelete.Id} nie można anulować.", ErrorType.Conflict);
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);

			return AppResult<Unit>.Success(Unit.Value, $"Usunięto zamówienie o numerze {issueToDelete.Id}.");
		}
	}
}