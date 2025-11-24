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
		InProgress = 1,//gdy pickowane
		IsShipped = 2,
		IsClosed = 3,
		Archived = 4,
		ConfirmedToLoad = 5,
		ChangingPallet = 6,
		NotComplete = 7,
		Pending = 8,//gdy przetwarzane w trakcie tworzenia
	}
}
