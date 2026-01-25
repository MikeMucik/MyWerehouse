using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class ReduceAllocationService : IReduceAllocationService
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IEventCollector _eventCollector;
		public ReduceAllocationService(IPickingTaskRepo pickingTaskRepo,
			IEventCollector eventCollector)
		{
			_pickingTaskRepo = pickingTaskRepo;
			_eventCollector = eventCollector;
		}
		public async Task ReduceAllocation(Issue issue, int productId, string userId)
		{
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issue.Id, productId);
			foreach (var item in pickingTasks)
			{
				item.PickingStatus = PickingStatus.Cancelled;
				_eventCollector.Add(new CreateHistoryPickingNotification(new HistoryDataPicking
				(
								item.Id,
								item.VirtualPallet.PalletId,
								item.IssueId,
								item.ProductId,
								item.Quantity,
								0,
								PickingStatus.Allocated,
								item.PickingStatus,
								userId,
								DateTime.UtcNow
				)));
			}
			throw new NotImplementedException();
		}
	}
}
