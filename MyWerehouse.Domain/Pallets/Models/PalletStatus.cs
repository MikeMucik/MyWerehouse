using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Pallets.Models
{
	public enum PalletStatus
	{
		New = 0, //Just created, without products
		Available = 1, //Putted to location from status instock or other manner comeback to available
		Damaged = 2,
		OnHold = 3, //To check
		Loaded = 4,
		ToPicking = 5, //source Pallet
		Archived = 6,
		Receiving = 7,
		InStock = 8, //Created with goods ready to use
		LockedForIssue = 9,//Assigned to an issue but not yet confirmed
		Picking = 10, //destination, during picking
		Cancelled = 11, //Cancelled
		ReversePicking = 12,//Created from picking
		ToIssue = 13,
	}
}
