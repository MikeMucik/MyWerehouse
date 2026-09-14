using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter
{
	public class GetReceiptsByFilterHandler(IReceiptReadService receiptReadService) : IRequestHandler<GetReceiptsByFilterQuery, AppResult<PagedResult<ReceiptSimplyDTO>>>
	{
		private readonly IReceiptReadService _receiptReadService = receiptReadService;

		public async Task<AppResult<PagedResult<ReceiptSimplyDTO>>> Handle(GetReceiptsByFilterQuery request, CancellationToken ct)
		{
			var receipts = await _receiptReadService.GetReceiptsByFilter(request.Filter, request.CurrentPage, request.PageSize, ct);
			if (receipts.TotalCount == 0) return AppResult<PagedResult<ReceiptSimplyDTO>>.Fail($"No receipts to display.");
			return AppResult<PagedResult<ReceiptSimplyDTO>>.Success(receipts);
		}
	}
}
