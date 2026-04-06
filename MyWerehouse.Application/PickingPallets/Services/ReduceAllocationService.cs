using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class ReduceAllocationService : IReduceAllocationService
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPalletRepo _palletRepo;
		public ReduceAllocationService(IPickingTaskRepo pickingTaskRepo, IPalletRepo palletRepo)
		{
			_pickingTaskRepo = pickingTaskRepo;			
			_palletRepo = palletRepo;
		}
		public async Task ReduceAllocation(Issue issue, Guid productId,int quantity, string userId)
		{
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issue.Id, productId);
			foreach (var pickingTask in pickingTasks)
			{
				if (quantity <= 0) break;

				if (quantity > 0)
				{
					if (pickingTask.RequestedQuantity > quantity)
					{
						pickingTask.ReduceQuantity(quantity);
						quantity = 0;
						var sourcePallet = await _palletRepo.GetPalletByIdAsync(pickingTask.VirtualPallet.PalletId);//
						pickingTask.AddHistory(userId, sourcePallet.Id, sourcePallet.PalletNumber,issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
					}
					else
					{
						quantity -= pickingTask.RequestedQuantity;
						pickingTask.Cancel(userId, issue.IssueNumber);
					}
				}				
			}			
		}
	}
}
