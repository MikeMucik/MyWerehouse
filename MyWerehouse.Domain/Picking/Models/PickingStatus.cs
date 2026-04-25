using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Picking.Models
{
	public enum PickingStatus
	{
		Available = 0,
		Allocated = 1,
		Picked = 2,
		Correction = 3,//to trzeba dodać w reduce i 
		Cancelled = 4,
		PickedPartially = 5,
	}
}
