using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public enum IssueStatus
	{
		New = 0,
		InProgress = 1,
		IsShipped = 2,
		IsClosed = 3,
		Archived = 4,
		ConfirmedToLoad = 5,
		ChangingPallet = 6,
	}
}
