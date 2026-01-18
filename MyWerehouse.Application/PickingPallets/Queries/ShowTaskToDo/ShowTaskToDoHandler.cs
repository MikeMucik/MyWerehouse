using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public class ShowTaskToDoHandler:IRequestHandler<ShowTaskToDoQuery, List<AllocationDTO>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IAllocationRepo _allocationRepo;
		public ShowTaskToDoHandler(IPickingPalletRepo pickingPalletRepo,
			IAllocationRepo allocationRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_allocationRepo = allocationRepo;
		}
		public async Task<List<AllocationDTO>> Handle (ShowTaskToDoQuery request, CancellationToken ct)
		{
			var palletVirtualId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletSourceScannedId);
			var allocations = await _allocationRepo.GetAllocationListAsync(palletVirtualId, request.PickingDate);
			//mapper??
			return allocations.Select(allocation => new AllocationDTO
			{
				AllocationId = allocation.Id,
				IssueId = allocation.IssueId,
				SourcePalletId = allocation.VirtualPallet.Pallet.Id,
				ProductId = allocation.VirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
				PickingStatus = allocation.PickingStatus,
				RequestedQuantity = allocation.Quantity,
				BestBefore = allocation.BestBefore
			}).ToList();
		}
	}
	
}
