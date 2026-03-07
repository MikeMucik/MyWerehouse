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
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.UpdatePallet
{
	public class UpdatePalletHandler(IPalletRepo palletRepo,
		IMapper mapper,
		WerehouseDbContext werehouseDbContext,
		IProductRepo productRepo) : IRequestHandler<UpdatePalletCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<AppResult<Unit>> Handle(UpdatePalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var existingPallet = await _palletRepo.GetPalletByIdAsync(request.UpdatingPallet.Id);
				if (existingPallet == null)
					return AppResult<Unit>.Fail($"Paleta o numerze {request.UpdatingPallet.Id} nie istnieje.", ErrorType.NotFound);
						
				
				foreach (var pop in request.UpdatingPallet.ProductsOnPallet)
				{
					if (!await _productRepo.IsExistProduct(pop.ProductId))
						return AppResult<Unit>.Fail($"Produkt o numerze {pop.ProductId} nie istnieje.", ErrorType.NotFound);
				}

				var updatedProducts = request.UpdatingPallet.ProductsOnPallet
					.Select(p => _mapper.Map<ProductOnPallet>(p)).ToList()
					.ToList();

				existingPallet.Update(request.UserId, updatedProducts, request.UpdatingPallet.Status);
				
				await _werehouseDbContext.SaveChangesAsync(ct);

				await transaction.CommitAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, $"Paleta {request.UpdatingPallet.Id} została zaktualizowana.");
				
			}
		
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				//return PalletResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				throw;
			}			
		}
	}
}
//_mapper.Map(request.UpdatingPallet, existingPallet);
//CollectionSynchronizer.SynchronizeCollection(
//	existingPallet.ProductsOnPallet,
//	request.UpdatingPallet.ProductsOnPallet,
//	a => a.Id,
//	a => a.Id,
//	dto =>
//	{
//		var newProduct = _mapper.Map<ProductOnPallet>(dto);
//		newProduct.PalletId = existingPallet.Id;
//		return newProduct;
//	},
//	(dto, entity) =>
//	{
//		var originalPalletId = entity.PalletId;  // Zapisz oryginalne FK przed mapowaniem
//		_mapper.Map(dto, entity);  // Mapuj resztę
//		entity.PalletId = originalPalletId;
//	});
//var oldProducts = existingPallet.ProductsOnPallet
//	.GroupBy(p => p.ProductId)
//	.ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
//var newProducts = request.UpdatingPallet.ProductsOnPallet
//	.GroupBy(p => p.ProductId)
//	.ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

//var allProductIds = oldProducts.Keys.Union(newProducts.Keys);
//foreach (var productId in allProductIds)
//{
//	oldProducts.TryGetValue(productId, out var oldQty);
//	newProducts.TryGetValue(productId, out var newQty);
//	var delta = newQty - oldQty;
//	if (delta > 0)
//	{
//		_eventCollector.Add(new ChangeStockNotification(
//				StockChangeType.Increase,
//				[new StockItemChange(productId, delta)]));
//	}
//	if (delta < 0)
//	{
//		_eventCollector.Add(new ChangeStockNotification(
//				StockChangeType.Decrease,
//				[new StockItemChange(productId, Math.Abs(delta))]
//			));
//	}
//}

//List<int> ids = updatedProducts.Select(a => a.ProductId).ToList();
//if( await _productRepo.EnsureAllExist(ids)) { throw new NotFoundProductException("nieprawidłowy produkt"); }
//var updatedProducts = request.UpdatingPallet.ProductsOnPallet
//	.Select(p => (p.ProductId, p.Quantity, p.BestBefore))
//	.ToList();
