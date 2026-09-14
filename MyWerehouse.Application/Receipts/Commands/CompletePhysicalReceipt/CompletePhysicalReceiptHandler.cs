using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt
{
	public class CompletePhysicalReceiptHandler(IUnitOfWork unitOfWork,
		IReceiptRepo receiptRepo) : IRequestHandler<CompletePhysicalReceiptCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(CompletePhysicalReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId, ct);
			if (receipt == null)
				return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");

			receipt.CompletePhysicalReceipt(request.UserId);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Physical receipt completed and ready for verification.");
		}
	}
}
