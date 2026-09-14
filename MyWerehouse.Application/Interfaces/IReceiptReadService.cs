using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Receipts.Queries.GetReceiptById;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Domain.Receiving.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IReceiptReadService
	{
		Task<ReceiptDTO?> GetReceiptById(Guid id, CancellationToken ct);
		Task<PagedResult<ReceiptSimplyDTO>> GetReceiptsByFilter(
			IssueReceiptSearchFilter filter,
			int pageNumber,
			int PageSize,
			CancellationToken ct);
	}
}
