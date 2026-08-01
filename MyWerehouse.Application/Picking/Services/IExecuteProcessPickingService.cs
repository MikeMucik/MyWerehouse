using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.Services
{
	public interface IExecuteProcessPickingService
	{
		//Task<ProcessPickingActionResult> ExecuteProcessPicking(Pallet sourcePallet, Issue issue, Guid productId,
		//	int quantityToPick, string userId, PickingTask pickingTask, PickingCompletion pickingCompletion, int locationId);
		Task<ProcessPickingActionResult> ExecuteProcessPicking(Pallet sourcePallet, PickingTask pickingTask,
		   int quantityToPick, string userId, int locationId);
	}
}
