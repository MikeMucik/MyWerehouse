using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Receviving.Events
{
	public record ChangeStatusReceiptNotification(
		int ReceiptId,
		ReceiptStatus ReceiptStatus,
		string UserId) : IDomainEvent;
	
}
