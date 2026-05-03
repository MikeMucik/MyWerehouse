using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Pallets.Commands.UpdatePallet
{
	public class UpdatePalletHandler(IPalletRepo palletRepo,		
		WerehouseDbContext werehouseDbContext,
		IProductRepo productRepo) : IRequestHandler<UpdatePalletCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;		
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<AppResult<Unit>> Handle(UpdatePalletCommand request, CancellationToken ct)
		{
			var existingPallet = await _palletRepo.GetPalletByIdAsync(request.UpdatingPallet.Id);
			if (existingPallet == null)
				return AppResult<Unit>.Fail($"Paleta o numerze {request.UpdatingPallet.PalletNumber} nie istnieje.", ErrorType.NotFound);
			foreach (var pop in request.UpdatingPallet.ProductsOnPallet)
			{
				if (!await _productRepo.IsExistProduct(pop.ProductId))
					return AppResult<Unit>.Fail($"Produkt o numerze {pop.ProductId} nie istnieje.", ErrorType.NotFound);
			}
			var updatedProducts1 = new List<ProductOnPallet>();
			foreach (var product in request.UpdatingPallet.ProductsOnPallet)
			{
				var updatetedProduct = ProductOnPallet.Create(product.ProductId,
					product.PalletId, product.Quantity, product.DateAdded, product.BestBefore);
				updatedProducts1.Add(updatetedProduct);
			}
			var snapShot = existingPallet.Location.ToSnapshot();
			existingPallet.Update(request.UserId, updatedProducts1, request.UpdatingPallet.Status, snapShot);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Paleta {request.UpdatingPallet.PalletNumber} została zaktualizowana.");
		}
	}
}

