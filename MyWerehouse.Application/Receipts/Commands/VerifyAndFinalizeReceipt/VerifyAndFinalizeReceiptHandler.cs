using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public class VerifyAndFinalizeReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo) : IRequestHandler<VerifyAndFinalizeReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(VerifyAndFinalizeReceiptCommand request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");

			// W obecnej wersji portfolio weryfikacja oznacza ręczne potwierdzenie zgodności przyjęcia.
			receipt.VerifiedReceipt(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
			return AppResult<Unit>.Success(Unit.Value, "Receipt pallets were verified and are ready for use.");
		}
	}
}
