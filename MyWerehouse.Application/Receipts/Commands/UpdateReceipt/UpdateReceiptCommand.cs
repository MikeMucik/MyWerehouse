using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public record UpdateReceiptCommand(Guid Id, UpdateReceiptDTO DTO): IRequest<AppResult<Unit>>;	
}
