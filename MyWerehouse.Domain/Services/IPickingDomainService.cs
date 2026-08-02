using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Services
{
	public interface IPickingDomainService
	{
		PickingTask GetSingleHandPickingTask(IReadOnlyCollection<PickingTask> tasks, Guid issueId, Guid productId);

		(int QuantityToPick, DateOnly? BestBefore) ReallocateForEmergencyPicking(IReadOnlyCollection<PickingTask> tasks,
			int availableQuantity, string userId, DateTime now, Guid issueId, Guid productId, Guid palletId, string palletNumber);
		IReadOnlyList<PickingTask> PrepareHandPickingTasks(IReadOnlyCollection<PickingTask> activeTasks, Guid issueId, string userId,
		DateTime now, DateOnly pickingDay);
		(List<VirtualPallet>, List<PickingTask>) ListVirtualPalletPickingTaskToCancel(IReadOnlyCollection<VirtualPallet> listVirtualPallets,Guid issueId, string userId, DateTime now);

	}
}
