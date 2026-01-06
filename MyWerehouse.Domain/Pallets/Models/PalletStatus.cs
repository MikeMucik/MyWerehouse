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
		InTransit = 9,//do zlecenia ale jeszcze nie zatwierdzona
		Picking = 10, //destination	
		Cancelled = 11, //anulowana				  
		ReversePicking = 12,//stworzona z pickingu
	}
}
