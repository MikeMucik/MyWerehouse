using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreateNewPalletHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo,
		IMapper mapper,
		IMediator mediator,
		IProductRepo productRepo,
		ILocationRepo locationRepo) : IRequestHandler<CreateNewPalletCommand, PalletResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;
		private readonly IPublisher _mediator = mediator;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<PalletResult> Handle(CreateNewPalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				foreach (var product in request.DTO.ProductsOnPallet)
				{
					if (!await _productRepo.IsExistProduct(product.ProductId))
						throw new NotFoundProductException(product.ProductId);
				}
				var newIdForPallet = await _palletRepo.GetNextPalletIdAsync();
				//var pallet = _mapper.Map<Pallet>(request.DTO);
				var listProducts = new List<ProductOnPallet>();
				foreach (var product in request.DTO.ProductsOnPallet)
				{
					var newProduct = new ProductOnPallet
					{
						ProductId = product.ProductId,
						Quantity = product.Quantity,
						DateAdded = product.DateAdded,
						BestBefore = product.BestBefore,
					};
					listProducts.Add(newProduct);
				}
				var pallet = new Pallet(newIdForPallet, DateTime.UtcNow, listProducts);
				_palletRepo.AddPallet(pallet);
				var location = await _locationRepo.GetLocationByIdAsync(1);
				pallet.AssignToWarehouse(location, request.UserId);

				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				//inventory
				var stockChanges = pallet.ProductsOnPallet
					.GroupBy(p => p.ProductId)
					.Select(g => new StockItemChange(g.Key, g.Sum(x => x.Quantity)))
					.ToList();
				if (stockChanges.Count != 0)
				{
					await _mediator.Publish(new ChangeStockNotification(
						StockChangeType.Increase, stockChanges), ct);
				}
				return PalletResult.Ok($"Dodano paletę {newIdForPallet} do stanu magazynowego, uaktualniono stan magazynowy.", newIdForPallet);
			}
			catch (NotFoundProductException epe)
			{
				await transaction.RollbackAsync(ct);
				return PalletResult.Fail(epe.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return PalletResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.", ex.Message);
			}
		}
	}
}
