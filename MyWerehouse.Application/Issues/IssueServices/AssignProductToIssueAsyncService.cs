using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pickings.Services;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public class AssignProductToIssueAsyncService(
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IVirtualPalletRepo virtualPalletRepo,
		IProductRepo productRepo,
		IPalletRepo palletRepo,
		IInventoryRepo inventoryRepo) : IAssignProductToIssueService
	{
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		public async Task<AssignProductToIssueResult> AssignGoodsToIssue(Issue issue, IssueItemDTO issueItem, IssueAllocationPolicy policy,
			List<Pallet>? oldAssignedPallets, string userId, CancellationToken ct)
		{
			var product = await _productRepo.GetProductByIdAsync(issueItem.ProductId, ct);
			if (product == null)
			{
				return AssignProductToIssueResult.Fail(
					"The specified product does not exist.",
					issueItem.ProductId,
					issueItem.Quantity);
			}
			oldAssignedPallets ??= [];//Full pallets containing the specified product, released during issue modification but temporarily retained for this operation
			var oldPalletCount = oldAssignedPallets.Count;
			//1. Validate stock availability
			//Available in the database
			var globallyAvailable = await _inventoryRepo.GetAllocatableQuantityAsync(issueItem.ProductId, issueItem.BestBefore, ct);
			//Available from the updated pallets temporarily held for this operation
			var reusableQuantity = oldAssignedPallets
				.Sum(p => p.GetProductQuantity(issueItem.ProductId));
			var totalAvailable = globallyAvailable + reusableQuantity;
			if (issueItem.Quantity > totalAvailable)
			{
				return AssignProductToIssueResult.Fail(
					$"Insufficient quantity of product {issueItem.ProductId}. The product was not added to the issue.",
					issueItem.ProductId,
					product.SKU,
					issueItem.Quantity,
					totalAvailable);
			}
			issue.BeginAllocation();
			//2. Allocate full pallets, applying the date requirement where applicable
			var requiredFullPallets = 0;
			var palletFullSelected = new List<Pallet>();
			var missingPalletsCount = 0;
			switch (policy)
			{
				case IssueAllocationPolicy.FullPalletFirst:
					requiredFullPallets = product.CalculateFullPalletCount(issueItem.Quantity);
					missingPalletsCount = requiredFullPallets - oldPalletCount;
					palletFullSelected = await SelectFullPallets(product, issueItem.BestBefore, oldAssignedPallets, requiredFullPallets, missingPalletsCount, ct);
					break;

				default:
					return AssignProductToIssueResult.Fail(
						$"Allocation policy {policy} is not supported.",
						issueItem.ProductId,
						product.SKU,
						issueItem.Quantity,
						totalAvailable);
			}
			var quantityFromPallets = palletFullSelected.Sum(p => p.GetProductQuantity(issueItem.ProductId));
			var rest = issueItem.Quantity - quantityFromPallets;
			// Guard against an invalid allocation plan
			if (rest < 0)
			{
				return AssignProductToIssueResult.Fail(
					"Allocated more product than requested.",
					issueItem.ProductId,
					product.SKU,
					issueItem.Quantity,
					totalAvailable);
			}
			//3. Retrieve available virtual pallets
			var availableVirtualPalletsQuery = await _virtualPalletRepo.GetVirtualPalletsByBBAsync(issueItem.ProductId, issueItem.BestBefore, ct);
			//4. Create a picking task for the remainder if rest > 0
			if (rest > 0)
			{
				var newPickingTaskFromRest = await _addPickingTaskToIssueService.AddPickingTasksToIssue(
					palletFullSelected, availableVirtualPalletsQuery, issue,
					issueItem.ProductId, rest, issueItem.BestBefore, userId, ct);
				if (newPickingTaskFromRest.Success is false)
				{
					return AssignProductToIssueResult.Fail(
						newPickingTaskFromRest.Message,
						issueItem.ProductId,
						product.SKU,
						issueItem.Quantity,
						totalAvailable);
				}
			}
			issue.AssignPallets(palletFullSelected, userId);
			return AssignProductToIssueResult.Ok(
				$"Product {product.SKU} was added to the issue.",
				issueItem.ProductId,
				product.SKU,
				palletFullSelected,
				issueItem.Quantity,
				totalAvailable);
		}
		//Full pallets first
		private async Task<List<Pallet>> SelectFullPallets(Product product, DateOnly? bestBefore, List<Pallet> reusablePalletsForProduct, int requiredFullPallets, int missingPalletsCount, CancellationToken ct)
		{
			List<Pallet> missingPallets = [];
			if (missingPalletsCount > 0)
			{
				missingPallets = await _palletRepo.GetMissingFullPallets(product.Id, product!.CartonsPerPallet, bestBefore, missingPalletsCount, ct);
			}
			List<Pallet> allNecessaryPallets = [.. reusablePalletsForProduct
				.Concat(missingPallets)
				.DistinctBy(p => p.Id)
				.Take(requiredFullPallets)];
			return allNecessaryPallets;
		}
		//Only FullPalletFirst is currently supported; other strategies can be added as separate allocation policies.
	}
}
