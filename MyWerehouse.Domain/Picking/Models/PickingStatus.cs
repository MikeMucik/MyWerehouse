using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Picking.Models
{
	public enum PickingStatus
	{
		Available = 0, //handPickingTasks
		Allocated = 1, //PlannedPickingTasks
		Picked = 2,//wykonany task
		CorrectionPicking = 3,//task po redukcji
		Cancelled = 4,//anulowany task
		PickedPartially = 5,//pobrany częściowo 
	}
}
