using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipts
{
	public record GetReceiptsQuery(IssueReceiptSearchFilter Filter) : IRequest<List<ReceiptDTO>>;
}
