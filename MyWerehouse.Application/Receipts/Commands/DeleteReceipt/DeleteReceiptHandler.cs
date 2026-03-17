using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.DeleteReceipt
{
	public class DeleteReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IPalletRepo palletRepo) : IRequestHandler<DeleteReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AppResult<Unit>> Handle(DeleteReceiptCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			{
				try
				{
					var receipt = await _receiptRepo.GetReceiptOnlyByIdAsync(request.ReceiptId);
					if (receipt == null) return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);
					if (receipt.Delete(request.UserId))
					{
						_receiptRepo.DeleteReceipt(receipt);
						return AppResult<Unit>.Success(Unit.Value, "Anulowano przyjęcie z bazy");
					}

					var listPalletsOfReceipt = await _palletRepo.GetPalletsByReceiptId(request.ReceiptId);
					bool canCancel = true;
					foreach (var pallet in listPalletsOfReceipt)
					{
						canCancel = pallet.CanBeCancelled();
						if (!canCancel)
						{
							return AppResult<Unit>.Fail("Nie można anulować przyjęcia, palety w obiegu magazynu.");
						}
					}
					foreach (var pallet in listPalletsOfReceipt)
					{
						pallet.CancelFromReceipt(request.UserId);
					}
					receipt.Cancel(request.UserId);
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					return AppResult<Unit>.Success(Unit.Value, "Anulowano przyjęcie wraz z paletami z bazy");
				}				
				catch (DomainException exd)
				{
					await transaction.RollbackAsync(ct);
					throw;
					//return AppResult<Unit>.Fail(exd.Message);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync(ct);
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return AppResult<Unit>.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
	}
}
