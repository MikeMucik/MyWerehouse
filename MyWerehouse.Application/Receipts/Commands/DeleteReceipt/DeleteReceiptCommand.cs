using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Receipts.Commands.DeleteReceipt
{
	public record DeleteReceiptCommand(int ReceiptId, string UserId):IRequest<ReceiptResult>;	
}
