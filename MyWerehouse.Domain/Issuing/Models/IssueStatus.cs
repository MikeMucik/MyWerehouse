using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Issuing.Models
{
	public enum IssueStatus
	{
		New = 0,//Can be deleted
		InProgress = 1,//When picked
		IsShipped = 2,//Loaded
		PickingShortage = 3, //Physical stock shortage detected during picking
		Archived = 4,//Archived
		ConfirmedToLoad = 5,//Confirmed for loading
		ChangingPallet = 6,//A pallet was replaced
		RequiresCorrection = 7,//Incomplete and requires correction, based on system data
		Pending = 8,//During modification
		Cancelled = 9,//Cancelled
	}
}
