using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.DTOs;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public record UpdateReceiptCommand(ReceiptDTO DTO, string UserId): IRequest<ReceiptResult>;	
}
