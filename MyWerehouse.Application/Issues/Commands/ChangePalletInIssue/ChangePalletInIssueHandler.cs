using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading
{
	public class ChangePalletInIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IPalletRepo palletRepo,
		IMediator mediator) : IRequestHandler<ChangePalletInIssueCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMediator _mediator = mediator;

		public async Task<IssueResult> Handle(ChangePalletInIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				if (request.OldPalletId == request.NewPalletId)
				{
					throw new PalletException("Nie można podmienić paletę na tą samą");
				}
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
					?? throw new IssueException(request.IssueId);
				var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(request.OldPalletId);
				var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(request.NewPalletId);
				if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
				{
					throw new PalletException("Jedna z podanych palet nie istnieje.");
				}
				if (palletToRemoveFromIssue.IssueId != request.IssueId)
				{
					throw new PalletException("Paleta do usunięcia nie należy do zlecenia.");
				}
				if (palletToAddingIssue.IssueId != null ||
					(palletToAddingIssue.Status != PalletStatus.Available &&
					palletToAddingIssue.Status != PalletStatus.InStock))
				{
					throw new PalletException("Nowej palety nie można przypisać do zlecenia, błędny status.");
				}
				var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
				var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
				if (productOnOldPallet is null)
					throw new PalletException("Paleta usuwana nie zawiera produktów.");

				if (productOnNewPallet is null)
					throw new PalletException("Nowa paleta nie zawiera produktów.");

				if (productOnOldPallet != productOnNewPallet)
					throw new PalletException("Nie można podmienić palet z różnymi produktami.");				
				palletToAddingIssue.IssueId = issue.Id;
				palletToAddingIssue.Status = PalletStatus.ToIssue;
				issue.Pallets.Add(palletToAddingIssue);

				palletToRemoveFromIssue.IssueId = null;
				palletToRemoveFromIssue.Status = PalletStatus.Available;
				issue.Pallets.Remove(palletToRemoveFromIssue);
				issue.IssueStatus = IssueStatus.ChangingPallet;
				issue.PerformedBy = request.UserId;
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreatePalletOperationNotification(
						palletToRemoveFromIssue.Id,
						palletToRemoveFromIssue.LocationId,
						ReasonMovement.Correction,
						issue.PerformedBy,
						PalletStatus.Available,
						null
					), ct);
				await _mediator.Publish(new CreatePalletOperationNotification(
							palletToAddingIssue.Id,
							palletToAddingIssue.LocationId,
							ReasonMovement.ToLoad,
							issue.PerformedBy,
							PalletStatus.ToIssue,
							null
						), ct);

				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);

				return IssueResult.Ok("Podmieniono palety.", productOnOldPallet.Value);
			}
			catch (PalletException ep)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ep.Message);
			}
			catch (IssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return IssueResult.Fail("Operacaja się nie powiodła.");
			}
		}
	}

}
