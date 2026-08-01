using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.Services
{
	public class AddPickingTaskToIssueService : IAddPickingTaskToIssueService
	{
		private readonly IProductRepo _productRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IDateTimeProvider _dateTimeProvider;
		public AddPickingTaskToIssueService(
			IProductRepo productRepo,
			IVirtualPalletRepo virtualPalletRepo,
			IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IDateTimeProvider dateTimeProvider)
		{
			_productRepo = productRepo;
			_virtualPalletRepo = virtualPalletRepo;
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet vp, Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId)
		{
			var now = _dateTimeProvider.UtcNow;
			var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated,
				productId, bestBefore, null, issue.IssueDateTimeSend.AddDays(-2), 0);
			_pickingTaskRepo.AddPickingTask(pickingTask);
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(vp.PalletId);
			if (sourcePallet == null)
				return AddPickingTaskToIssueResult.Fail("Source pallet was not found.");
			pickingTask.AddHistoryPicking(userId, null, null, PickingStatus.Available, 0, now);
			return AddPickingTaskToIssueResult.Ok(pickingTask);
		}
		public async Task<AddPickingTaskToIssueResult> AddPickingTasksToIssue(List<Pallet>? pallets, List<VirtualPallet>? virtualPallets,
			Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId)
		{
			var now = _dateTimeProvider.UtcNow;
			virtualPallets ??= [];
			// Palety nie są zapisane w bazie, bo cały proces odbywa się w jednym handlerze przed SaveChanges.			
			var pickingTasks = new List<PickingTask>(); //dla result 			
			//z dostępnych palet do pickingu	
			foreach (var vp in virtualPallets)
			{
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				var pickingTask = PickingTask.CreatePickingTaskForIssue(
					vp, issue, taken, productId, issue.IssueDateTimeSend.AddDays(-2), bestBefore, userId, now);
				_pickingTaskRepo.AddPickingTask(pickingTask);
				pickingTasks.Add(pickingTask);
				quantity -= taken;
				if (quantity <= 0)
					break;					
			}
			//new pallets for picking					
			var usedPalletsId = pallets?
				.Select(p => p.Id)
				.ToHashSet() ?? new HashSet<Guid>();
			var availablePallets = await _palletRepo.GetAvailablePalletsExcluding(productId, bestBefore, usedPalletsId);
			
			foreach (var palletToPicking in availablePallets)
			{
				if (quantity <= 0) break;
				var virtualPallet = VirtualPallet.CreateFromPallet(palletToPicking, palletToPicking.ProductsOnPallet.Single().Quantity, palletToPicking.LocationId, now);
				palletToPicking.AssignToPicking(userId, palletToPicking.Location.ToSnapshot()); //from new pallet for picking
				var vp = _virtualPalletRepo.AddPalletToPicking(virtualPallet);
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				var pickingTask = PickingTask.CreatePickingTaskForIssue(
					vp, issue, taken, productId, issue.IssueDateTimeSend.AddDays(-2), bestBefore, userId, now);
				_pickingTaskRepo.AddPickingTask(pickingTask);
				pickingTasks.Add(pickingTask);
				quantity -= taken;
				if (quantity <= 0) break;				
			}
			//if there is not enough product, a message will be sent to the user - for DoPlannedPicking
			if (quantity > 0)
			{
				var productSKU = await _productRepo.GetSKUForProductAsync(productId);
				return AddPickingTaskToIssueResult.Fail($"No more stock is available for product {productSKU}; a picking task cannot be created.");
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}
	}
}
