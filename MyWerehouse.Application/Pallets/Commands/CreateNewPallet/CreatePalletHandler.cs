using MediatR;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreatePalletHandler(IUnitOfWork unitOfWork,
		IPalletRepo palletRepo, ILocationRepo locationRepo, IDateTimeProvider dateTimeProvider, IPalletNumberAllocator palletNumberAllocator)
		: IRequestHandler<CreatePalletCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		private readonly IPalletNumberAllocator _palletNumberAllocator = palletNumberAllocator;

		public async Task<AppResult<Unit>> Handle(CreatePalletCommand request, CancellationToken ct)
		{
			var location = await _locationRepo.GetLocationByIdAsync(request.RampNumber, ct);
			if (location == null) return AppResult<Unit>.Fail("The specified ramp does not exist.");
			var newIdForPallet = (await _palletNumberAllocator.ReserveAsync(1, ct)).Single();
			var now = _dateTimeProvider.UtcNow;
			var pallet = Pallet.Create(newIdForPallet, request.RampNumber, now);
			foreach (var product in request.DTO.ProductsOnPallet)
			{
				pallet.AddProduct(product.ProductId, product.Quantity, now, product.BestBefore);
			}
			_palletRepo.AddPallet(pallet);
			var snapShot = location.ToSnapshot();
			pallet.AssignToWarehouse(location.Id, snapShot, request.UserId);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Pallet {newIdForPallet} was added to warehouse stock and inventory was updated.");
		}
	}
}
