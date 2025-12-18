using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public enum ReversePickingStatus
	{
		Pending = 0,
		InProgress = 1,
		Completed = 2,
		Cancelled = 3,
		Failed = 4,
		Archaive = 5,
	}
}
