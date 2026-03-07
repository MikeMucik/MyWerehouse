using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public record VerifyAndFinalizeReceiptCommand(Guid ReceiptId, string UserId) : IRequest<AppResult<Unit>>;

}
