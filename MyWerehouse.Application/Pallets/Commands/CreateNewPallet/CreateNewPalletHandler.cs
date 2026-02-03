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
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreateNewPalletHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo,
		IMapper mapper,
		IMediator mediator,
		IProductRepo productRepo) : IRequestHandler<CreateNewPalletCommand, PalletResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;
		private readonly IPublisher _mediator = mediator;
		private readonly IProductRepo _productRepo = productRepo;

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
				var pallet = _mapper.Map<Pallet>(request.DTO);
				pallet.Id = newIdForPallet;
				pallet.LocationId = 1;
				pallet.Status = PalletStatus.InStock;
				_palletRepo.AddPallet(pallet);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
								ReasonMovement.New, request.UserId, PalletStatus.InStock, null), ct);
				//inventory
				var stockChanges = pallet.ProductsOnPallet
					.GroupBy(p => p.ProductId)
					.Select(g => new StockItemChange(g.Key,	g.Sum(x => x.Quantity)))
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
