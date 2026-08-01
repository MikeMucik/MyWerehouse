using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public class AssignProductToIssueAsyncService(
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IGetProductCountService getProductCountService,
		IVirtualPalletRepo virtualPalletRepo,
		IProductRepo productRepo,
		IPalletRepo palletRepo) : IAssignProductToIssueService
	{
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IGetProductCountService _getProductCountService = getProductCountService;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		public async Task<AssignProductToIssueResult> AssignGoodsToIssue(Issue issue, IssueItemDTO issueItem, IssueAllocationPolicy policy,
			List<Pallet>? oldAssignedPallets, string userId)
		{
			issue.BeginAllocation();
			var product = await _productRepo.GetProductByIdAsync(issueItem.ProductId);
			if (product == null)
			{
				return AssignProductToIssueResult.Fail("The specified product does not exist.", issueItem.ProductId);
			}
			oldAssignedPallets ??= [];//pełne palety z wskazanym produktem anulowane przy modyfikacji zlecenia, ale trzymane tymczasowo tylko do tej operacji
			var oldPalletCount = oldAssignedPallets.Count;
			//1. dostępność towaru	- walidacja
			var totalAvailable = await _getProductCountService.GetProductCountAsync(issueItem.ProductId, issueItem.BestBefore);
			if (issueItem.Quantity > totalAvailable)
			{
				return AssignProductToIssueResult.Fail($"Insufficient quantity of product {issueItem.ProductId}. The product was not added to the issue."
						, issueItem.ProductId, product.SKU, issueItem.Quantity, totalAvailable);
			}
			//2. Przydzielanie pełnych lub/z datą palet
			var requiredFullPallets = 0;
			var palletFullSelected = new List<Pallet>();
			var missingPalletsCount = 0;
			switch (policy)
			{
				case IssueAllocationPolicy.FullPalletFirst:
					requiredFullPallets = product.CalculateFullPalletCount(issueItem.Quantity);
					missingPalletsCount = requiredFullPallets - oldPalletCount;
					palletFullSelected = await SelectFullPallets(product, issueItem.BestBefore, oldAssignedPallets, requiredFullPallets, missingPalletsCount);
					break;

				default:
					return AssignProductToIssueResult.Fail($"Allocation policy {policy} is not supported.");
			}
			var quantityFromPallets = palletFullSelected.Sum(p => p.GetProductQuantity(issueItem.ProductId));
			var rest = issueItem.Quantity - quantityFromPallets;// ta linijka potrzebna
			if (rest < 0) return AssignProductToIssueResult.Fail("Allocated more product than requested.");
			//3. pobierz dostępne virtualPallet;
			var availableVirtualPalletsQuery = await _virtualPalletRepo.GetVirtualPalletsByBBAsync(issueItem.ProductId, issueItem.BestBefore);
			//4. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
			if (rest > 0)
			{
				var newPickingTaskFromRest = await _addPickingTaskToIssueService.AddPickingTasksToIssue(
					palletFullSelected, availableVirtualPalletsQuery, issue,
					issueItem.ProductId, rest, issueItem.BestBefore, userId);
				if (newPickingTaskFromRest.Success is false)
				{
					return AssignProductToIssueResult.Fail(newPickingTaskFromRest.Message, issueItem.ProductId, product.SKU, issueItem.Quantity, totalAvailable);
				}
			}
			issue.AssignPallets(palletFullSelected, userId);
			return AssignProductToIssueResult.Ok($"Product {product.SKU} was added to the issue.", issueItem.ProductId, product.SKU, palletFullSelected);
		}
		//pełne palety first
		private async Task<List<Pallet>> SelectFullPallets(Product product, DateOnly? bestBefore, List<Pallet> reusablePalletsForProduct, int requiredFullPallets, int missingPalletsCount)
		{
			List<Pallet> missingPallets = [];
			if (missingPalletsCount > 0)
			{
				missingPallets = await _palletRepo.GetMissingFullPallets(product.Id, product!.CartonsPerPallet, bestBefore, missingPalletsCount);
			}
			// Czy tą operację lepiej zrobic na Dictionary ?
			List<Pallet> allNecessaryPallets = [.. reusablePalletsForProduct
				.Concat(missingPallets)
				.DistinctBy(p => p.Id)
				.Take(requiredFullPallets)];
			return allNecessaryPallets;
		}
		//Obecnie wspierana jest polityka FullPalletFirst; pozostałe strategie mogą zostać dodane jako osobne polityki alokacji.
	}
}
