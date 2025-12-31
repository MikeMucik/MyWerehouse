using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreateNewPalletHandler : IRequestHandler<CreateNewPalletCommand, PalletResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IMapper _mapper;
		private readonly IMediator _mediator;
		private readonly IProductRepo _productRepo;

		public CreateNewPalletHandler(WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IMapper mapper,
			IMediator mediator,
			IProductRepo productRepo)
		{
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_mapper = mapper;
			_mediator = mediator;
			_productRepo = productRepo;
		}
		public async Task<PalletResult> Handle(CreateNewPalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{				
				var newIdForPallet = await _palletRepo.GetNextPalletIdAsync();
				var pallet = _mapper.Map<Pallet>(request.DTO);
				pallet.Id = newIdForPallet;
				pallet.LocationId = 1;
				pallet.Status = PalletStatus.InStock;
				if (!await _productRepo.IsExistProduct(pallet.ProductsOnPallet.First().ProductId))
					throw new InvalidProductException(pallet.ProductsOnPallet.First().ProductId);
				pallet.ProductsOnPallet = [new ProductOnPallet {
				ProductId = request.DTO.ProductsOnPallet.First().ProductId,

				Quantity = request.DTO.ProductsOnPallet.First().Quantity,
			}];
				_palletRepo.AddPallet(pallet);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
								ReasonMovement.New, request.UserId, PalletStatus.InStock, null), ct);
				//inventory
				var item = pallet.ProductsOnPallet.FirstOrDefault();
				await _mediator.Publish(new ChangeStockNotification(StockChangeType.Increase, [new StockItemChange(item.ProductId, item.Quantity)]), ct);
				return PalletResult.Ok($"Dodano paletę {newIdForPallet} do stanu magazynowego, uaktualniono stan magazynowy.", newIdForPallet);
			}
			catch (InvalidProductException epe)
			{
				await transaction.RollbackAsync(ct);
				return PalletResult.Fail(epe.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return PalletResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}
}
