using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Receipts.DTOs;

namespace MyWerehouse.Application.Receipts.Queries.GetReceipt
{
	public record GetReceiptByIdQuery(int ReceiptId) :IRequest<ReceiptDTO>;	
}
