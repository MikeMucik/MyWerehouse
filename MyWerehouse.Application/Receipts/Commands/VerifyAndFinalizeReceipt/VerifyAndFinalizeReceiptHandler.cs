using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public class VerifyAndFinalizeReceiptHandler(IUnitOfWork unitOfWork,
		IReceiptRepo receiptRepo) : IRequestHandler<VerifyAndFinalizeReceiptCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(VerifyAndFinalizeReceiptCommand request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId, cancellationToken);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");

			// In the current version of the portfolio, verification means manual confirmation that the acceptance is compliant.
			receipt.VerifiedReceipt(request.UserId);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return AppResult<Unit>.Success(Unit.Value, "Receipt pallets were verified and are ready for use.");
		}
	}
}
