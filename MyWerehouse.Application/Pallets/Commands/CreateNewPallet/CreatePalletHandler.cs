using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreatePalletHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo, ILocationRepo locationRepo, IDateTimeProvider dateTimeProvider)
		: IRequestHandler<CreatePalletCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<Unit>> Handle(CreatePalletCommand request, CancellationToken ct)
		{
			var location = await _locationRepo.GetLocationByIdAsync(request.RampNumber);
			if (location == null) return AppResult<Unit>.Fail("The specified ramp does not exist.");
			var newIdForPallet = await _palletRepo.GetNextPalletIdAsync();
			var now = _dateTimeProvider.UtcNow;
			var pallet = Pallet.Create(newIdForPallet, request.RampNumber, now);
			foreach (var product in request.DTO.ProductsOnPallet)
			{
				pallet.AddProduct(product.ProductId, product.Quantity, now, product.BestBefore);
			}
			_palletRepo.AddPallet(pallet);
			var snapShot = location.ToSnapshot();
			pallet.AssignToWarehouse(location.Id, snapShot, request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Pallet {newIdForPallet} was added to warehouse stock and inventory was updated.");
		}
	}
}
