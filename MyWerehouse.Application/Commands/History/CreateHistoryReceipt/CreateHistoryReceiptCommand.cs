using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Commands.History.CreateHistoryReceipt
{
	public record CreateHistoryReceiptCommand(
		int ReceiptId,
		ReceiptStatus ReceiptStatus,
		string UserId):INotification;
	
}
