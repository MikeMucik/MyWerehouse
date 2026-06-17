using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Issuing.Models
{
	public enum IssueStatus
	{
		New = 0,//można delete
		InProgress = 1,//gdy pickowane
		IsShipped = 2,//załadowane
		//IsClosed = 3,//zamknięte - niepotrzebne
		Archived = 4,//w archiwum
		ConfirmedToLoad = 5,//zatwierdzone do załadunku
		ChangingPallet = 6,//nastąpiła podmiana palety
		RequiresCorrection = 7,//niekompletne do poprawki
		Pending = 8,//gdy jest modify
		Cancelled = 9,//anulowane
	}
}
