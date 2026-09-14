using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Receipts.Commands.CancelReceipt
{
	public class CancelReceiptHandler(IReceiptRepo receiptRepo,
		IUnitOfWork unitOfWork) : IRequestHandler<CancelReceiptCommand, AppResult<Unit>>
	{
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;

		public async Task<AppResult<Unit>> Handle(CancelReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceipForCancelByIdAsync(request.ReceiptId, ct);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");
			receipt.Cancel(request.UserId);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Receipt and its pallets were cancelled.");
		}
	}
}
