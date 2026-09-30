using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Histories.Models
{
	public enum ReasonForPallet
	{
		New = 0, // New pallet
		Received = 1,//New pallet in a receipt
		Picking = 2,//Source pallet for picking
		Moved = 3,//The pallet was moved to another location
		Correction = 4,//The pallet was corrected
		Merge = 5,//The pallet was merged with another pallet
		ToLoad = 6,//Pallet added to an issue
		Loaded = 7,//Loaded pallet
		CancelIssue = 8,//The pallet was removed from the issue because the issue was cancelled
		ReversePicking = 9,//Pallet affected by reverse picking
	}
}
