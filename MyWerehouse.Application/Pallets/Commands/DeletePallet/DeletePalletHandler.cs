using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Events.ChangeStock;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.DeletePallet
{
	public class DeletePalletHandler : IRequestHandler<DeletePalletCommand, PalletResult>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IMediator _mediator;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IEventCollector _eventCollector;
		public DeletePalletHandler(IPalletRepo palletRepo,
			IPalletMovementRepo palletMovementRepo,
			IMediator mediator,
			WerehouseDbContext werehouseDbContext,
			IEventCollector eventCollector)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			_mediator = mediator;
			_werehouseDbContext = werehouseDbContext;
			_eventCollector = eventCollector;
		}
		public async Task<PalletResult> Handle(DeletePalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId)
						?? throw new PalletException($"Nie ma palety o numerze {request.PalletId}");
				if (!await _palletMovementRepo.CanDeletePalletAsync(pallet.Id))
					throw new PalletException($"Palety o numerze {request.PalletId} nie można usunąć");

				foreach (var pop in pallet.ProductsOnPallet)
				{
					_eventCollector.Add(new ChangeStockNotification(
						StockChangeType.Decrease, // cofamy wpływ palety
						[new StockItemChange(pop.ProductId, pop.Quantity)]
					));
				}
				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
								ReasonMovement.Correction, request.UserId, PalletStatus.Cancelled, null));
				await _werehouseDbContext.SaveChangesAsync(ct);
				foreach (var ev in _eventCollector.Events)
				{
					await _mediator.Publish(ev, ct);
				}
				//_eventCollector.Clear();
				_palletRepo.DeletePallet(pallet);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return PalletResult.Ok("Paleta została usunięta");
			}
			catch (PalletException epr)
			{
				await transaction.RollbackAsync(ct);
				return PalletResult.Fail(epr.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return PalletResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
			finally
			{
				_eventCollector.Clear();
			}
		}
	}
}
