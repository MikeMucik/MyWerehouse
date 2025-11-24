using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public record VerifyAndFinalizeReceiptCommand(int ReceiptId, string UserId) : IRequest<ReceiptResult>;

}
