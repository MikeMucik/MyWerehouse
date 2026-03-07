using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public record GetReceiptsQuery(IssueReceiptSearchFilter Filter) : IRequest<AppResult<List<ReceiptDTO>>>;
}
