using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkAsLoadedHandler :IRequestHandler<MarkAsLoadedCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		public MarkAsLoadedHandler(WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo)
		{
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
		}
		public async Task<AppResult<Unit>> Handle(MarkAsLoadedCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
				if (pallet == null)
					return AppResult<Unit>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
				if (pallet.Status == PalletStatus.Loaded)
					return AppResult<Unit>.Fail($"Paleta {request.PalletId} jest już załadowana.", ErrorType.Conflict);
				var allowedStatuses = new[]
				{
					PalletStatus.ToIssue,
					PalletStatus.InTransit,
					PalletStatus.Available,
					PalletStatus.InStock
				};
				if (!allowedStatuses.Contains(pallet.Status))
					return AppResult<Unit>.Fail("Paleta nie ma statusu do załadowania");
				pallet.AddHistory(PalletStatus.Loaded, ReasonMovement.Loaded, request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, $"Paleta {request.PalletId} została załadowana.");
			}			
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return AppResult<Unit>.Fail("Wystąpił nieoczekiwany błąd podczas operacji.", ErrorType.Technical);
			}
		}
	}
}
