using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkAsLoadedHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<MarkAsLoadedCommand, AppResult<MarkPalletAsLoadedResponseDTO>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<MarkPalletAsLoadedResponseDTO>> Handle(MarkAsLoadedCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
				return AppResult<MarkPalletAsLoadedResponseDTO>.Fail($"Wskazana paleta nie istnieje.", ErrorType.NotFound);		
			pallet.MarkAsLoaded(request.UserId, pallet.Location.ToSnapshot());
			await _werehouseDbContext.SaveChangesAsync(ct);
			var respone = new MarkPalletAsLoadedResponseDTO
			{
				PalletId = pallet.Id,
				PalletNumber = pallet.PalletNumber,
				NewStatus = pallet.Status,
				LoadedAt = _dateTimeProvider.UtcNow,
			};
			return AppResult<MarkPalletAsLoadedResponseDTO>.Success(respone, $"Paleta {pallet.PalletNumber} załadowana.");
		}
	}
}
