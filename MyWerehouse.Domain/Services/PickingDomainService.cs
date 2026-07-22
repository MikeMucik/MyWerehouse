using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Picking.PickingExceptions;

namespace MyWerehouse.Domain.Services
{
	public class PickingDomainService : IPickingDomainService
	{
		public PickingDomainService() { }

		public PickingCompletion DetermineCompletion(int requestedQuantity, int pickedQuantity)
		{
			if (requestedQuantity > pickedQuantity)
			{
				return PickingCompletion.Partial;
			}
			return PickingCompletion.Full;
		}

		public PickingTask GetSingleHandPickingTask(IReadOnlyCollection<PickingTask> tasks, Guid issueId, Guid productId)
		{
			if (tasks.Count == 0)
			{
				throw new PickingTaskNotFoundDomainException(issueId, productId);
			}
			if (tasks.Count > 1)
			{
				throw new TooManyTaskDomainException(issueId, productId);
			}
			var task = tasks.Single();
			if (task.VirtualPalletId.HasValue)
			{
				throw new InvalidPickingStrategyDomainException(issueId, productId);
			}
			return task;
		}

		public void ReduceAllocation(IReadOnlyCollection<PickingTask> tasks, int quantity, string userId, DateTime now)
		{
			var orderedTasks = tasks //Reduce from the smallest tasks
				.OrderBy(a => a.RequestedQuantity - a.PickedQuantity)
				.ThenBy(a => a.Id);
			foreach (var pickingTask in orderedTasks)
			{
				if (quantity <= 0) break;
				var remainningQuantity = pickingTask.RequestedQuantity - pickingTask.PickedQuantity;
				if (remainningQuantity > quantity)
				{
					pickingTask.ReduceQuantity(quantity, userId, now);
					quantity = 0;
				}
				else
				{
					quantity -= remainningQuantity;
					pickingTask.Cancel(userId, now);
				}
			}
		}		
	}
}
