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

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkAsLoadedHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo) : IRequestHandler<MarkAsLoadedCommand, AppResult<MarkPalletAsLoadedResponeDTO>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<MarkPalletAsLoadedResponeDTO>> Handle(MarkAsLoadedCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
				return AppResult<MarkPalletAsLoadedResponeDTO>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
			if (pallet.Status == PalletStatus.Loaded)
				return AppResult<MarkPalletAsLoadedResponeDTO>.Fail($"Paleta {request.PalletId} jest już załadowana.", ErrorType.Conflict);
			var allowedStatuses = new[]
				{
					PalletStatus.ToIssue,
					PalletStatus.LockedForIssue,
				};
			if (!allowedStatuses.Contains(pallet.Status))
				return AppResult<MarkPalletAsLoadedResponeDTO>.Fail("Paleta nie ma statusu do załadowania");
			pallet.MarkAsLoaded(request.UserId, pallet.Location.ToSnapshot());
			await _werehouseDbContext.SaveChangesAsync(ct);
			var respone = new MarkPalletAsLoadedResponeDTO
			{
				PalletId = pallet.Id,
				PalletNumber = pallet.PalletNumber,
				NewStatus = pallet.Status,
				LoadedAt = DateTime.UtcNow,				
			};
			return AppResult<MarkPalletAsLoadedResponeDTO>.Success(respone, "Paleta załadowana.");
		}
	}
}
