using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.DeleteReceipt
{
	public class DeleteReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletMovementRepo palletMovementRepo,
		IPalletRepo palletRepo,
		IPublisher mediator) : IRequestHandler<DeleteReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletMovementRepo _palletMovementRepo = palletMovementRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPublisher _mediator = mediator;

		public async Task<ReceiptResult> Handle(DeleteReceiptCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			{
				try
				{
					var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
					?? throw new NotFoundReceiptException(request.ReceiptId);
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

					if (receipt.ReceiptStatus == ReceiptStatus.Planned)
					{
						_werehouseDbContext.Receipts.Remove(receipt);
						return ReceiptResult.Ok("Usunięto zlecenie", receipt.Id);
					}
					else
					{
						receipt.ReceiptStatus = ReceiptStatus.Cancelled;
						receipt.PerformedBy = request.UserId;
						receipt.ReceiptDateTime = DateTime.UtcNow;

						var restPallet = new List<string>();
						//usuwanie palet które jeszcze nie weszły w "życie" magazynu					
						foreach (var pallet in receipt.Pallets.ToList())
						{
							if (!await _palletMovementRepo.CanDeletePalletAsync(pallet.Id))
								restPallet.Add(pallet.Id);
							else pallet.Status = PalletStatus.Cancelled;
						}
						if (restPallet.Count > 0)
						{
							await _werehouseDbContext.SaveChangesAsync(ct);
							await transaction.CommitAsync(ct);
							return ReceiptResult.Fail($"Nie można usunąć wszystkich palet z przyjęcia. Przyjęcie ma teraz status anulowane. Lista nie skasowanych palet : {string.Join(",", restPallet)}");
						}

						receipt.ReceiptStatus = ReceiptStatus.Cancelled;
					}
					;
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, ReceiptStatus.Cancelled,
						request.UserId), ct);
					return ReceiptResult.Ok("Anulowano przyjęcie wraz z paletami z bazy", request.ReceiptId);
				}
				catch (NotFoundReceiptException erp)
				{
					await transaction.RollbackAsync(ct);
					return ReceiptResult.Fail(erp.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync(ct);
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
	}
}
