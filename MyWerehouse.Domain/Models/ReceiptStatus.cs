using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public enum ReceiptStatus
	{
		Planned = 0,
		InProgress = 1,
		PhysicallyCompleted = 2,
		Verified = 3,
		Closed = 4,//czy potrzebne tylko jeśli nie usuwamy przyjęcia i zostawiamy historie
		Cancelled =5,//czy potrzebne
	}
}
