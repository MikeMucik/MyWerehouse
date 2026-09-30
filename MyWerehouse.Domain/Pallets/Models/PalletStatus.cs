using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Pallets.Models
{
	public enum PalletStatus
	{
		Available = 0,
		ToIssue = 1,
		Damaged = 2,
		OnHold = 3,
		Loaded = 4,
		ToPicking = 5, //source
		Archived = 6,
		Receiving = 7,
		InStock = 8,
		LockedForIssue = 9,//Assigned to an issue but not yet confirmed
		Picking = 10, //destination, during picking
		Cancelled = 11, //Cancelled
		ReversePicking = 12,//Created from picking
		New = 13, 
	}
}
