using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.CancelReceipt
{
	public class CancelReceiptCommandHandler(IReceiptRepo receiptRepo,
		WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo) : IRequestHandler<CancelReceiptCommand, AppResult<Unit>>
	{
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;


		public async Task<AppResult<Unit>> Handle(CancelReceiptCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);

			var receipt = await _receiptRepo.GetReceiptOnlyByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);

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
				pallet.DetachFromReceipt(request.UserId, pallet.Location.ToSnopShot());
			}
			receipt.Cancel(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Anulowano przyjęcie wraz z paletami z bazy");
		}
	}
}
