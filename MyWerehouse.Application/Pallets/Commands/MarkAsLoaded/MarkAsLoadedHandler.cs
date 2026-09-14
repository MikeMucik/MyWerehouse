using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkAsLoadedHandler(
		IUnitOfWork unitOfWork,
		IPalletRepo palletRepo,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<MarkAsLoadedCommand, AppResult<MarkPalletAsLoadedResponseDTO>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<MarkPalletAsLoadedResponseDTO>> Handle(MarkAsLoadedCommand request, CancellationToken ct)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId, ct);
			if (pallet == null)
				return AppResult<MarkPalletAsLoadedResponseDTO>.Fail($"The specified pallet does not exist.");
			pallet.MarkAsLoaded(request.UserId, pallet.Location.ToSnapshot());
			await _unitOfWork.SaveChangesAsync(ct);
			var respone = new MarkPalletAsLoadedResponseDTO
			{
				PalletId = pallet.Id,
				PalletNumber = pallet.PalletNumber,
				NewStatus = pallet.Status,
				LoadedAt = _dateTimeProvider.UtcNow,
			};
			return AppResult<MarkPalletAsLoadedResponseDTO>.Success(respone, $"Pallet {pallet.PalletNumber} was loaded.");
		}
	}
}
