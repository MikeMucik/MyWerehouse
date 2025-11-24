using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt
{
	public record CompletePhysicalReceiptCommand(int ReceiptId, string UserId): IRequest<ReceiptResult>;
}
