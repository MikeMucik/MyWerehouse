using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public record UpdateReceiptCommand(int ReceiptId, ReceiptDTO Receipt, string UserId): IRequest<ReceiptResult>;	
}
