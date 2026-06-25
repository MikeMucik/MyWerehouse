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
		PickingShortage = 3, //nie wystarczająca ilość towaru - wykryto podczas pickingu - fizycznie
		Archived = 4,//w archiwum
		ConfirmedToLoad = 5,//zatwierdzone do załadunku
		ChangingPallet = 6,//nastąpiła podmiana palety
		RequiresCorrection = 7,//niekompletne do poprawki - wykryto na podstawie systemu
		Pending = 8,//gdy jest modify
		Cancelled = 9,//anulowane
	}
}
