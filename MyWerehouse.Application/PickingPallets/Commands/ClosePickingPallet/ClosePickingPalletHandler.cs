using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet
{
	public class ClosePickingPalletHandler(IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		IPickingPalletRepo pickingPalletRepo,
		WerehouseDbContext werehouseDbContext,
		IMediator mediator) : IRequestHandler<ClosePickingPalletCommand, PickingResult>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IMediator _mediator = mediator;

		public async Task<PickingResult>  Handle(ClosePickingPalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId) ?? throw new NotFoundPalletException(request.PalletId);
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId) ?? throw new NotFoundIssueException(request.PalletId);
				if (pallet.Status != PalletStatus.Picking)
					return PickingResult.Fail($"Palety {pallet.Id} nie można zamknąć. Błędny status palet"); 
				//to powyżej można rozszerzyć na konkretne przypaski
				_pickingPalletRepo.ClosePickingPallet(request.PalletId, request.IssueId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreatePalletOperationNotification(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.Picking,
							request.UserId,
							PalletStatus.ToIssue,
							null
						), ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, request.UserId),ct);
				return PickingResult.Ok($"Zamknięto paletę, dołączono do zlecenia {issue.Id}.");
			}
			catch (NotFoundPalletException exp)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(exp.Message);
			}
			catch (NotFoundIssueException exo)
			{
				await transaction.RollbackAsync(ct);
				return PickingResult.Fail(exo.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return PickingResult.Fail("Nastąpił nieoczekiwany błąd");
			}
		}
	}
}
