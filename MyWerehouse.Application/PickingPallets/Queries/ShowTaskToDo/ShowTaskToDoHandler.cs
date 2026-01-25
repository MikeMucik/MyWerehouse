using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.ViewModels.PickingTaskModels;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public class ShowTaskToDoHandler:IRequestHandler<ShowTaskToDoQuery, List<PickingTaskDTO>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		public ShowTaskToDoHandler(IPickingPalletRepo pickingPalletRepo,
			IPickingTaskRepo pickingTaskRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_pickingTaskRepo = pickingTaskRepo;
		}
		public async Task<List<PickingTaskDTO>> Handle (ShowTaskToDoQuery request, CancellationToken ct)
		{
			var palletVirtualId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletSourceScannedId);
			var pickingTasks = await _pickingTaskRepo.GetPickingTaskListAsync(palletVirtualId, request.PickingDate);
			//mapper??
			return pickingTasks.Select(pickingTask => new PickingTaskDTO
			{
				PickingTaskId = pickingTask.Id,
				IssueId = pickingTask.IssueId,
				SourcePalletId = pickingTask.VirtualPallet.Pallet.Id,
				ProductId = pickingTask.VirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
				PickingStatus = pickingTask.PickingStatus,
				RequestedQuantity = pickingTask.Quantity,
				BestBefore = pickingTask.BestBefore
			}).ToList();
		}
	}
	
}
