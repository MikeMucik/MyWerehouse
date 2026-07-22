using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Services
{
	public interface IPickingDomainService
	{		
		PickingTask GetSingleHandPickingTask(IReadOnlyCollection<PickingTask> tasks, Guid issueId, Guid productId);
		PickingCompletion DetermineCompletion(int requestedQuantity, int pickedQuantity);
		void ReduceAllocation(IReadOnlyCollection<PickingTask> tasks, int quantity, string userId, DateTime now);

	}
}
