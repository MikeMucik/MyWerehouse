using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Picking.PickingExceptions;

namespace MyWerehouse.Domain.Services
{
	public class PickingDomainService : IPickingDomainService
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;
		public PickingDomainService(IPickingTaskRepo pickingTaskRepo)
		{
			_pickingTaskRepo = pickingTaskRepo;
		}

		public async Task<PickingTask> GetSingleHandPickingTask(Guid issueId, Guid productId)
		{

			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issueId, productId);
			if(pickingTasks.Count == 0)
			{
				throw new PickingTaskNotFoundDomainException(issueId, productId);
			}
			
			if (pickingTasks.Count > 1)
			{
				throw new TooManyTaskDomainException(issueId, productId);
			}
			
			if(pickingTasks.Any(a => a.VirtualPallet != null))
			{
				throw new InvalidPickingStrategyDomainException(issueId, productId);
			}
			return pickingTasks[0];
		}
	}
}
