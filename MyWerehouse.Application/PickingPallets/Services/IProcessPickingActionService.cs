using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public interface IProcessPickingActionService
	{
		Task<ProcessPickingActionResult> ProcessPicking(Pallet sourcePallet,
		Issue issue, Guid productId, int quantityToPick,
		string userId, PickingTask pickingTask,
		PickingCompletion pickingCompletion);
	}
}
