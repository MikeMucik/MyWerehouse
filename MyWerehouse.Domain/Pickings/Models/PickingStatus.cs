using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Pickings.Models
{
	public enum PickingStatus
	{
		Available = 0, //handPickingTasks
		Allocated = 1, //PlannedPickingTasks
		Picked = 2,//Completed task
		CorrectionPicking = 3,//Task after quantity reduction
		Cancelled = 4,//Cancelled task
		PickedPartially = 5,//Partially picked
	}
}
