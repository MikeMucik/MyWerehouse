using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.DeleteReceipt
{
	public class DeleteReceiptHandler : IRequestHandler<DeleteReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IPublisher _mediator;
		public DeleteReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IPalletMovementRepo palletMovementRepo,
			IPalletRepo palletRepo,
			IPublisher mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_palletMovementRepo = palletMovementRepo;
			_palletRepo = palletRepo;
			_mediator = mediator;
		}
		public async Task<ReceiptResult> Handle(DeleteReceiptCommand request, CancellationToken cancellationToken)
		{
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(cancellationToken))
			{
				try
				{
					var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
					?? throw new ReceiptNotFoundException($"Brak przyjęcia o numerze{request.ReceiptId}");
					if (receipt.ReceiptStatus == ReceiptStatus.Verified)
					{
						return ReceiptResult.Fail("Nie można usunąć zweryfikowanego przyjęcia");
					}
					if (!(receipt.ReceiptStatus == ReceiptStatus.Planned
						|| receipt.ReceiptStatus == ReceiptStatus.InProgress
						|| receipt.ReceiptStatus == ReceiptStatus.PhysicallyCompleted))
					{
						return ReceiptResult.Fail("Nieprawidłowy status przyjęcia");
					}
					receipt.ReceiptStatus = ReceiptStatus.Cancelled;
					receipt.PerformedBy = request.UserId;
					receipt.ReceiptDateTime = DateTime.UtcNow;

					var restPallet = new List<string>();
					//usuwanie palet które jeszcze nie weszły w "życie" magazynu					
					foreach (var pallet in receipt.Pallets.ToList())
					{
						//dodaj sprawdzenie czy można usunąć paletę??
						//bool canDelete = await _palletMovementRepo.CanDeletePalletAsync(pallet.Id);
						if (!await _palletMovementRepo.CanDeletePalletAsync(pallet.Id))
							restPallet.Add(pallet.Id);
						else pallet.Status = PalletStatus.Cancelled;
					}
					if (restPallet.Count > 0)
					{
						await _werehouseDbContext.SaveChangesAsync(cancellationToken);
						await transaction.CommitAsync(cancellationToken);
						return ReceiptResult.Fail($"Nie można usunąć wszystkich palet z przyjęcia. Przyjęcie ma teraz status anulowane. Lista nieksasowanych palet : {string.Join(",", restPallet)}");
					}						
					
					await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, ReceiptStatus.Cancelled, request.UserId), cancellationToken);
					
					receipt.ReceiptStatus = ReceiptStatus.Cancelled;
					await _werehouseDbContext.SaveChangesAsync(cancellationToken);
					await transaction.CommitAsync(cancellationToken);
					return ReceiptResult.Ok("Usunięto przyjęcie wraz z paletami z bazy", request.ReceiptId);
				}
				catch (ReceiptNotFoundException erp)
				{
					await transaction.RollbackAsync(cancellationToken);
					return ReceiptResult.Fail(erp.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync(cancellationToken);
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
	}
}
