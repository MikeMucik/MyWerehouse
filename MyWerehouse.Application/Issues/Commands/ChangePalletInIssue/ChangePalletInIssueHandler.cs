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
				return AppResult<Unit>.Fail("Issue was not found.");
			var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(request.OldPalletId);
			if (palletToRemoveFromIssue is null)
				return AppResult<Unit>.Fail($"Pallet {request.OldPalletId}, which should be replaced, does not exist.");
			var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(request.NewPalletId);
			if (palletToAddingIssue is null)
				return AppResult<Unit>.Fail($"Replacement pallet {request.NewPalletId} does not exist.");
			var bestBefore = issue.IssueItems.Single(x=>x.ProductId == palletToRemoveFromIssue.ProductsOnPallet.Single().ProductId).BestBefore;
			issue.ReplacePalletInIssue(palletToRemoveFromIssue, palletToAddingIssue, request.UserId, bestBefore);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Pallets were replaced.");
		}
	}
}
