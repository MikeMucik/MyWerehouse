using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Receviving.Models
{
	public enum ReceiptStatus
	{
		Planned = 0,
		InProgress = 1,
		PhysicallyCompleted = 2,
		Verified = 3,
		Correction = 4,		
		Cancelled = 5,// gdy anuluje przyjęcie
		Deleted =6,
	}
}
