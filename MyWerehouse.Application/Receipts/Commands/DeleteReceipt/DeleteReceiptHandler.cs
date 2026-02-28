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
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Events;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.DeleteReceipt
{
	public class DeleteReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo) : IRequestHandler<DeleteReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<ReceiptResult> Handle(DeleteReceiptCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			{
				try
				{
					var receipt = await _receiptRepo.GetReceiptOnlyByIdAsync(request.ReceiptId)
					?? throw new NotFoundReceiptException(request.ReceiptId);

					if (receipt.Delete(request.UserId))
					{
						_receiptRepo.DeleteReceipt(receipt);
						return ReceiptResult.Ok("Anulowano przyjęcie z bazy", receipt.ReceiptNumber);
					}

					var listPalletsOfReceipt = await _palletRepo.GetPalletsByReceiptId(request.ReceiptId);
					bool canCancel = true;
					foreach (var pallet in listPalletsOfReceipt)
					{
						canCancel = pallet.CanBeCancelled();
						if (!canCancel)
						{
							return ReceiptResult.Fail("Nie można anulować przyjęcia, palety w obiegu magazynu.");
						}
					}
					foreach (var pallet in listPalletsOfReceipt)
					{
						pallet.CancelFromReceipt(request.UserId);
					}
					receipt.Cancel(request.UserId);
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					return ReceiptResult.Ok("Anulowano przyjęcie wraz z paletami z bazy", receipt.ReceiptNumber);
				}
				catch (NotFoundReceiptException erp)
				{
					await transaction.RollbackAsync(ct);
					return ReceiptResult.Fail(erp.Message);
				}
				catch (DomainException exd)
				{
					await transaction.RollbackAsync(ct);
					throw;
					//return ReceiptResult.Fail(exd.Message);
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
