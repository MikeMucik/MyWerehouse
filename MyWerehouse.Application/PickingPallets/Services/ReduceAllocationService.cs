using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
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
		public async Task ReduceAllocation(Issue issue, int productId,int quantity, string userId)
		{			
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issue.Id, productId) ?? throw new NotFoundPickingTaskException("Brak alokacji do redukcji");
			foreach (var pickingTask in pickingTasks)
			{
				if (quantity <= 0) break;

				if (quantity > 0)
				{
					if (pickingTask.RequestedQuantity > quantity)
					{
						pickingTask.RequestedQuantity -= quantity;
						quantity = 0;
						var historyPicking = new HistoryDataPicking
							(
								pickingTask.Id,
								pickingTask.VirtualPallet.PalletId,
								pickingTask.IssueId,
								pickingTask.ProductId,
								pickingTask.RequestedQuantity,
								0,
								PickingStatus.Correction,
								pickingTask.PickingStatus,
								userId,
								DateTime.UtcNow);
						_eventCollector.Add(new CreateHistoryPickingNotification(historyPicking));
					}
					else
					{
						quantity -= pickingTask.RequestedQuantity;
						pickingTask.PickingStatus = PickingStatus.Cancelled;
						pickingTask.RequestedQuantity = 0;
						var historyPicking = new HistoryDataPicking
							(
								pickingTask.Id,
								pickingTask.VirtualPallet.PalletId,
								pickingTask.IssueId,
								pickingTask.ProductId,
								pickingTask.RequestedQuantity,
								0,
								PickingStatus.Correction,
								pickingTask.PickingStatus,
								userId,
								DateTime.UtcNow);
						_eventCollector.Add(new CreateHistoryPickingNotification(historyPicking));
					}
				}				
			}			
		}
	}
}
