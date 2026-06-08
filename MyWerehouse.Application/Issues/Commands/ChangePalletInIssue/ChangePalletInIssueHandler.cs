using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
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
			if (_palletRepo.GetPalletByIdAsync(request.NewPalletId) is null)
				return AppResult<Unit>.Fail($"Paleta na którą chcesz wymienić o numerze {request.NewPalletId} nie istnieje.", ErrorType.NotFound);
			if (_palletRepo.GetPalletByIdAsync(request.OldPalletId) is null)
				return AppResult<Unit>.Fail($"Paleta którą chcesz podmienić o numerze {request.NewPalletId} nie istnieje.", ErrorType.NotFound);
			if (request.OldPalletId == request.NewPalletId)
				return AppResult<Unit>.Fail("Nie można podmienić paletę na tą samą", ErrorType.Conflict);
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(request.OldPalletId);
			var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(request.NewPalletId);
			if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
				return AppResult<Unit>.Fail("Jedna z podanych palet nie istnieje.", ErrorType.Conflict);
			if (palletToRemoveFromIssue.IssueId != request.IssueId)
				return AppResult<Unit>.Fail("Paleta do usunięcia nie należy do zlecenia.", ErrorType.Conflict);
			if (palletToAddingIssue.IssueId != null ||
				(palletToAddingIssue.Status != PalletStatus.Available &&
				palletToAddingIssue.Status != PalletStatus.InStock))
				return AppResult<Unit>.Fail("Nowej palety nie można przypisać do zlecenia, błędny status.", ErrorType.Conflict);
			var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			if (productOnOldPallet is null)
				return AppResult<Unit>.Fail("Paleta usuwana nie zawiera produktów.", ErrorType.NotFound);
			if (productOnNewPallet is null)
				return AppResult<Unit>.Fail("Nowa paleta nie zawiera produktów.", ErrorType.NotFound);
			if (productOnOldPallet != productOnNewPallet)
				return AppResult<Unit>.Fail("Nie można podmienić palet z różnymi produktami.", ErrorType.Conflict);
			palletToAddingIssue.ReserveToIssue(issue.Id, request.UserId, palletToAddingIssue.Location.ToSnapshot());
			issue.AttachPallet(palletToAddingIssue);
			palletToRemoveFromIssue.DetachToIssue(request.UserId, palletToRemoveFromIssue.Location.ToSnapshot(), Domain.Histories.Models.ReasonForPallet.Correction);
			issue.DetachPallet(palletToRemoveFromIssue);
			issue.ChangePalletInIssue(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value , "Podmieniono palety.");
		}
	}
}