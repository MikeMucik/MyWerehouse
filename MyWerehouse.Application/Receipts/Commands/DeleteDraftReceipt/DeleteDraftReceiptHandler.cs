using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Commands.DeleteDraftReceipt
{
	public class DeleteDraftReceiptHandler(IUnitOfWork unitOfWork,
		IReceiptRepo receiptRepo
		) : IRequestHandler<DeleteDraftReceiptCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(DeleteDraftReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceipForCancelByIdAsync(request.ReceiptId, ct);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");
			receipt.Delete(request.UserId);
			_receiptRepo.DeleteReceipt(receipt);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Receipt was deleted.");
		}
	}
}
