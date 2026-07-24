using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading
{
	public class ChangePalletInIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IPalletRepo palletRepo) : IRequestHandler<ChangePalletInIssueCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<Unit>> Handle(ChangePalletInIssueCommand request, CancellationToken ct)
		{
			//Można podmieniać tylko palety z jednym towarem, nie palety kompletacyjne			
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(request.OldPalletId);
			if (palletToRemoveFromIssue is null)
				return AppResult<Unit>.Fail($"Paleta którą chcesz podmienić o numerze {request.OldPalletId} nie istnieje.", ErrorType.NotFound);
			var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(request.NewPalletId);
			if (palletToAddingIssue is null)
				return AppResult<Unit>.Fail($"Paleta na którą chcesz wymienić o numerze {request.NewPalletId} nie istnieje.", ErrorType.NotFound);
			issue.ReplacePalletInIssue(palletToRemoveFromIssue, palletToAddingIssue, request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Podmieniono palety.");
		}
	}
}