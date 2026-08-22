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
using MyWerehouse.Domain.Services;

namespace MyWerehouse.Application.Picking.Services
{
	public class AddPickingTaskToIssueService : IAddPickingTaskToIssueService
	{
		private readonly IProductRepo _productRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IPickingDomainService _pickingDomainService;
		public AddPickingTaskToIssueService(
			IProductRepo productRepo,
			IVirtualPalletRepo virtualPalletRepo,
			IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IDateTimeProvider dateTimeProvider,
			IPickingDomainService pickingDomainService)
		{
			_productRepo = productRepo;
			_virtualPalletRepo = virtualPalletRepo;
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_dateTimeProvider = dateTimeProvider;
			_pickingDomainService = pickingDomainService;
		}

		public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet vp, Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(vp.PalletId, ct);
			if (sourcePallet == null)
				return AddPickingTaskToIssueResult.Fail("Source pallet was not found.");
			var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated,
				productId, bestBefore, null, issue.IssueDateTimeSend.AddDays(-2), 0);
			_pickingTaskRepo.AddPickingTask(pickingTask);

			pickingTask.AddHistoryPicking(userId, null, null, PickingStatus.Available, 0, now);
			return AddPickingTaskToIssueResult.Ok(pickingTask);
		}
		public async Task<AddPickingTaskToIssueResult> AddPickingTasksToIssue(List<Pallet>? pallets, List<VirtualPallet>? virtualPallets,
			Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			virtualPallets ??= [];
			var pickingTasks = new List<PickingTask>();
			var virtualPalletsAll = new List<VirtualPallet>();
			virtualPalletsAll.AddRange(virtualPallets);
			var sumFromVirtualPallets = 0;
			foreach (var vp in virtualPallets)
			{
				var quantityFromOldVirtualPallets = vp.RemainingQuantity;
				sumFromVirtualPallets += quantityFromOldVirtualPallets;
			}
			var quantityForNewVirtualPallet = quantity - sumFromVirtualPallets;
			if (quantityForNewVirtualPallet > 0)
			{
				var usedPalletsId = pallets?
					.Select(p => p.Id)
					.ToHashSet() ?? new HashSet<Guid>();

				var candidates = await _palletRepo.GetCandidates(productId, bestBefore, usedPalletsId, ct);
				var selectedIds = new List<Guid>();
				var remaining = quantityForNewVirtualPallet;
				foreach (var candidate in candidates)
				{
					if (remaining <= 0)
					{
						break;
					}
					selectedIds.Add(candidate.PalletId);
					remaining -= candidate.Quantity;
				}
				if (remaining > 0)
				{
					var productSku = await _productRepo.GetSKUForProductAsync(productId, ct);

					return AddPickingTaskToIssueResult.Fail(
						$"No more stock is available for product {productSku}; " +
						"a picking task cannot be created.");
				}
				var availablePallets = await _palletRepo.GetSelectedPallets(selectedIds, ct);

				foreach (var palletToPicking in availablePallets)
				{
					if (quantityForNewVirtualPallet <= 0) break;
					var virtualPallet = VirtualPallet.CreateFromPallet(palletToPicking, palletToPicking.ProductsOnPallet.Single().Quantity,
						palletToPicking.LocationId, now);
					palletToPicking.AssignToPicking(userId, palletToPicking.Location.ToSnapshot());
					var addedVirtualPallet = _virtualPalletRepo.AddPalletToPicking(virtualPallet);
					virtualPalletsAll.Add(addedVirtualPallet);
					quantityForNewVirtualPallet -= addedVirtualPallet.RemainingQuantity;
				}
			}
			var resultAllocation = _pickingDomainService.Allocate(issue, virtualPalletsAll, productId, quantity, bestBefore, userId, now);
			foreach (var task in resultAllocation.PickingTasks)
			{
				_pickingTaskRepo.AddPickingTask(task);
			}
			pickingTasks.AddRange(resultAllocation.PickingTasks);
			//if there is not enough product, a message will be sent to the user - for DoPlannedPicking
			if (resultAllocation.RemainingQuantity > 0)
			{
				var productSKU = await _productRepo.GetSKUForProductAsync(productId, ct);
				return AddPickingTaskToIssueResult.Fail($"No more stock is available for product {productSKU}; a picking task cannot be created.");
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}
	}
}
