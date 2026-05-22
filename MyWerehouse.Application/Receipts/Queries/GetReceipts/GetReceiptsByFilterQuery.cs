using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public class GetReceiptsByFilterQuery : IRequest<AppResult<PagedResult<ReceiptSimplyDTO>>>
	{
		public IssueReceiptSearchFilter Filter { get; set; } = new();
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
	};
}
