using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet
{
	public class ClosePickingPalletHandler(IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<ClosePickingPalletCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<Unit>> Handle(ClosePickingPalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
				if (pallet == null)
					return AppResult<Unit>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if (issue == null)
					return AppResult<Unit>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
				// chyba do domeny		
				//if (pallet.Status != PalletStatus.Picking)
				//	return AppResult<Unit>.Fail($"Palety {pallet.Id} nie można zamknąć. Błędny status palet");
				//to powyżej można rozszerzyć na konkretne przypadki				
				pallet.CloseAndAddPickingPallet(request.IssueId, request.UserId, pallet.Location);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, $"Zamknięto paletę, dołączono do zlecenia {issue.Id}.");
			}
			//catch (NotFoundPalletException exp)
			//{
			//	await transaction.RollbackAsync(ct);
			//	return PickingResult.Fail(exp.Message);
			//}
			//catch (NotFoundIssueException exo)
			//{
			//	await transaction.RollbackAsync(ct);
			//	return PickingResult.Fail(exo.Message);
			//}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				//return PickingResult.Fail("Nastąpił nieoczekiwany błąd");
				throw;
			}
		}
	}
}
//_pickingPalletRepo.ClosePickingPallet(request.PalletId, request.IssueId);
//await _mediator.Publish(new ChangeStatusOfPalletNotification(
//			pallet.Id,
//			pallet.LocationId,
//			pallet.LocationId,
//			ReasonMovement.Picking,
//			request.UserId,
//			PalletStatus.ToIssue,
//			null
//		), ct);
//await _mediator.Publish(new AddHistoryForIssueNotification(issue.Id, request.UserId),ct);
