using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class ReduceAllocationService : IReduceAllocationService
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;
		public ReduceAllocationService(IPickingTaskRepo pickingTaskRepo)
		{
			_pickingTaskRepo = pickingTaskRepo;			
		}
		public async Task ReduceAllocation(Issue issue, int productId,int quantity, string userId)
		{
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issue.Id, productId);
			foreach (var pickingTask in pickingTasks)
			{
				if (quantity <= 0) break;

				if (quantity > 0)
				{
					if (pickingTask.RequestedQuantity > quantity)
					{
						pickingTask.RequestedQuantity -= quantity;
						quantity = 0;
						pickingTask.AddHistory(userId, PickingStatus.Correction, pickingTask.PickingStatus, 0);
					}
					else
					{
						quantity -= pickingTask.RequestedQuantity;
						pickingTask.PickingStatus = PickingStatus.Cancelled;
						pickingTask.RequestedQuantity = 0;
						pickingTask.AddHistory(userId, PickingStatus.Correction, pickingTask.PickingStatus, 0);//
					}
				}				
			}			
		}
	}
}
