using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Picking.PickingExceptions;

namespace MyWerehouse.Domain.Services
{
	public class PickingDomainService : IPickingDomainService
	{
		public PickingDomainService() { }
		
		public PickingTask GetSingleHandPickingTask(IReadOnlyCollection<PickingTask> tasks, Guid issueId, Guid productId)
		{
			if (tasks.Any(x => x.PickingStatus == PickingStatus.Allocated || x.PickingStatus == PickingStatus.CorrectionPicking))
			{
				throw new InvalidPickingStrategyDomainException(issueId, productId);
			}
			var handTasks = tasks
				.Where(x => x.PickingStatus == PickingStatus.Available
				&& x.VirtualPalletId == null && x.RequestedQuantity > 0 )
				.ToList();
			if (handTasks.Count == 0)
			{
				throw new PickingTaskNotFoundDomainException(issueId, productId);
			}
			if (handTasks.Count > 1)
			{
				throw new TooManyTaskDomainException(issueId, productId);
			}
			var task = handTasks.Single();
			return task;
		}

		public (int QuantityToPick, DateOnly? BestBefore) ReallocateForEmergencyPicking(IReadOnlyCollection<PickingTask> tasks, int availableQuantity, string userId, DateTime now,
		Guid issueId, Guid productId, Guid palletId, string palletNumber)
		{
			if (availableQuantity <= 0)
				throw new InsufficientQuantityDomainException(palletId, palletNumber);
			var allocatedTask = tasks
				.Where(a => a.PickingStatus == PickingStatus.Allocated
				|| a.PickingStatus == PickingStatus.CorrectionPicking)
				.ToList();
			var neededQuantity = allocatedTask.Sum(a => a.RequestedQuantity - a.PickedQuantity); //-PickedQuantity for safety
			if (neededQuantity <= 0)
			{
				throw new NoNeededQuantityDomainException(issueId, productId);
			}
			var bestBefore = allocatedTask[0].BestBefore;
			var quantityToPick = Math.Min(neededQuantity, availableQuantity);
			ReduceAllocation(allocatedTask, quantityToPick, userId, now);
			return (quantityToPick, bestBefore);
		}

		private static void ReduceAllocation(IReadOnlyCollection<PickingTask> tasks, int quantity, string userId, DateTime now)
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

		public IReadOnlyList<PickingTask> PrepareHandPickingTasks(IReadOnlyCollection<PickingTask> activeTasks,Guid issueId, string userId, DateTime now, DateOnly pickingDay)
		{			
			var listToDoTasks = new List<PickingTask>();
			var list = activeTasks
					.GroupBy(p => p.ProductId)
					.Select(g => new
					{
						g.Key,
						TotalQuantity = g.Sum(p => p.RequestedQuantity),
						BestBefore = g.First().BestBefore
					}).ToList();
			foreach (var task in list)
			{
				var taskToDo = PickingTask.Create(null, issueId, task.TotalQuantity, PickingStatus.Available, task.Key,
					task.BestBefore, null, pickingDay, 0);
				listToDoTasks.Add(taskToDo);
			}
			foreach (var task in activeTasks)
			{
				task.Cancel(userId, now);
			}
			return listToDoTasks;
		}

		public (List<VirtualPallet>, List<PickingTask>) ListVirtualPalletPickingTaskToCancel(IReadOnlyCollection<VirtualPallet> listVirtualPallets, Guid issueId, string userId, DateTime now)
		{
			var listPickingTaskToCancel = new List<PickingTask>();
			var listVirtualPalletsToCancel = new List<VirtualPallet>();
			foreach (var vp in listVirtualPallets)
			{
				var pickingTaskToRemove = vp.PickingTasks
					.Where(a => (a.PickingStatus == PickingStatus.Allocated
					|| a.PickingStatus == PickingStatus.Available ||
					a.PickingStatus == PickingStatus.CorrectionPicking)&& a.IssueId == issueId)
					.ToList();
				foreach (var pickingTask in pickingTaskToRemove)
				{
					pickingTask.Cancel(userId, now);

					vp.PickingTasks.Remove(pickingTask);
					listPickingTaskToCancel.Add(pickingTask);
				}
				//usuń virtualPallet jeśli należy tylko do tego zlecenia
				if (vp.PickingTasks.Count == 0)
				{					
					vp.Pallet.ChangeStatus(PalletStatus.Available);
					listVirtualPalletsToCancel.Add(vp);
				}
			}
			return (listVirtualPalletsToCancel, listPickingTaskToCancel);
			//throw new NotImplementedException();
		}
	}
}
