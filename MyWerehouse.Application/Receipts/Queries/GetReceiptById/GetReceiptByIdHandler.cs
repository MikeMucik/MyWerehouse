using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptById
{
	public class GetReceiptByIdHandler(IReceiptReadService receiptReadService) : IRequestHandler<GetReceiptByIdQuery, AppResult<ReceiptDTO>>
	{
		private readonly IReceiptReadService _receiptReadService = receiptReadService;

		public async Task<AppResult<ReceiptDTO>> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptReadService.GetReceiptById(request.ReceiptId, cancellationToken);
			if (receipt == null) return AppResult<ReceiptDTO>.Fail($"Receipt {request.ReceiptId} was not found.");
			return AppResult<ReceiptDTO>.Success(receipt);
		}
	}
}
