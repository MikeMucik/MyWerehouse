using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading
{
	public class ChangePalletInIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IPalletRepo palletRepo) : IRequestHandler<ChangePalletInIssueCommand, AppResult<IssueResult>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<IssueResult>> Handle(ChangePalletInIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				if (_palletRepo.GetPalletByIdAsync(request.NewPalletId) is null)
					return AppResult<IssueResult>.Fail($"Paleta na którą chcesz wymienić o numerze {request.NewPalletId} nie istnieje.", ErrorType.NotFound);				
				if (_palletRepo.GetPalletByIdAsync(request.OldPalletId) is null)
					return AppResult<IssueResult>.Fail($"Paleta którą chcesz podmienić o numerze {request.NewPalletId} nie istnieje.", ErrorType.NotFound);
				if (request.OldPalletId == request.NewPalletId)
					return AppResult<IssueResult>.Fail("Nie można podmienić paletę na tą samą", ErrorType.Conflict);
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if (issue == null)
					return AppResult<IssueResult>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
				var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(request.OldPalletId);
				var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(request.NewPalletId);
				if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
					return AppResult<IssueResult>.Fail("Jedna z podanych palet nie istnieje.", ErrorType.Conflict);
				if (palletToRemoveFromIssue.IssueId != request.IssueId)
					return AppResult<IssueResult>.Fail("Paleta do usunięcia nie należy do zlecenia.", ErrorType.Conflict);
				if (palletToAddingIssue.IssueId != null ||
					(palletToAddingIssue.Status != PalletStatus.Available &&
					palletToAddingIssue.Status != PalletStatus.InStock))
					return AppResult<IssueResult>.Fail("Nowej palety nie można przypisać do zlecenia, błędny status.", ErrorType.Conflict);
				var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
				var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
				if (productOnOldPallet is null)
					return AppResult<IssueResult>.Fail("Paleta usuwana nie zawiera produktów.", ErrorType.NotFound);
				if (productOnNewPallet is null)
					return AppResult<IssueResult>.Fail("Nowa paleta nie zawiera produktów.", ErrorType.NotFound);
				if (productOnOldPallet != productOnNewPallet)
					return AppResult<IssueResult>.Fail("Nie można podmienić palet z różnymi produktami.", ErrorType.Conflict);				
				palletToAddingIssue.ReserveToIssue(issue , request.UserId, palletToAddingIssue.Location.ToSnopShot());
				issue.AttachPallet(palletToAddingIssue);
				palletToRemoveFromIssue.DetachToIssue(issue.Id, request.UserId, palletToRemoveFromIssue.Location.ToSnopShot(), Domain.Histories.Models.ReasonMovement.Correction);
				issue.DetachPallet(palletToRemoveFromIssue);
				issue.ChangePalletInIssue(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<IssueResult>.Success(IssueResult.Ok("Podmieniono palety.", productOnOldPallet.Value));
			}			
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return AppResult<IssueResult>.Fail("Operacaja się nie powiodła.");
			}
		}
	}

}
