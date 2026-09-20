using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pickings.Models;

namespace MyWerehouse.Application.Pickings.Services
{
	public interface IExecuteProcessPickingService
	{
		Task<ProcessPickingActionResult> ExecuteProcessPicking(Pallet sourcePallet, PickingTask pickingTask,
		   int quantityToPick, string userId, int locationId, CancellationToken ct);
	}
}
