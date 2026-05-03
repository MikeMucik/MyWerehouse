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
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkAsLoadedHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo) : IRequestHandler<MarkAsLoadedCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<Unit>> Handle(MarkAsLoadedCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
				return AppResult<Unit>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
			if (pallet.Status == PalletStatus.Loaded)
				return AppResult<Unit>.Fail($"Paleta {request.PalletId} jest już załadowana.", ErrorType.Conflict);
			var allowedStatuses = new[]
				{
					PalletStatus.ToIssue,
					PalletStatus.LockedForIssue,
					PalletStatus.Available,
					PalletStatus.InStock
				};
			if (!allowedStatuses.Contains(pallet.Status))
				return AppResult<Unit>.Fail("Paleta nie ma statusu do załadowania");
			pallet.ChangeStatus(PalletStatus.Loaded);
			pallet.AddHistory(ReasonMovement.Loaded, request.UserId, pallet.Location.ToSnapshot());
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Paleta {request.PalletId} została załadowana.");
		}
	}
}
