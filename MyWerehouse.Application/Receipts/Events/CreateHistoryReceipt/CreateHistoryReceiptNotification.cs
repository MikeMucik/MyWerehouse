using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt
{
	public record CreateHistoryReceiptNotification(
		int ReceiptId,
		ReceiptStatus ReceiptStatus,
		string UserId):INotification;
	
}
