using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Receiving.Models
{
	public enum ReceiptStatus
	{
		Planned = 0,//New
		InProgress = 1,//Receving
		PhysicallyCompleted = 2,//AfterReceiving Before InStock
		Verified = 3,//InStock
		Correction = 4,//Update made
		Cancelled = 5,//
		Deleted = 6,//
	}
}
