using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public class AssignProductToIssueAsyncService(
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IGetProductCountService getProductCountService,
		IGetNumberPalletsAndRestService getNumberPalletsAndRestService,
		IVirtualPalletRepo virtualPalletRepo,
		IProductRepo productRepo,
		IPalletRepo palletRepo) : IAssignProductToIssueService
	{
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IGetProductCountService _getProductCountService = getProductCountService;
		private readonly IGetNumberPalletsAndRestService _getNumberPalletsAndRestService = getNumberPalletsAndRestService;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;

		public async Task<AssignProductToIssueResult> AssignProductToIssue(Issue issue, IssueItemDTO issueLine, IssueAllocationPolicy policy,
			List<Pallet>? reusablePalletsForProduct, string userId)
		{
			if (issue.IssueStatus == IssueStatus.New)
				issue.ChangeStatus(IssueStatus.Pending);
			if (issue.IssueStatus != IssueStatus.Pending && issue.IssueStatus != IssueStatus.New &&
			issue.IssueStatus != IssueStatus.RequiresCorrection)
			{
				return AssignProductToIssueResult.Fail("Błąd statusu zlecenia.");
			}
			var product = await _productRepo.GetProductByIdAsync(issueLine.ProductId);
			if (product == null)
			{
				return AssignProductToIssueResult.Fail("Wskazany produkt nie istnieje.", issueLine.ProductId);
			}
			var productSKU = product.SKU;
			reusablePalletsForProduct ??= [];//protected null
			var oldCount = reusablePalletsForProduct.Count;

			//1. dostępność towaru	- walidacja
			var totalAvailable = await _getProductCountService.GetProductCountAsync(issueLine.ProductId, issueLine.BestBefore);
			if (issueLine.Quantity > totalAvailable)
			{
				return AssignProductToIssueResult.Fail($"Nie wystarczająca ilość produktu o numerze {issueLine.ProductId}. Asortyment nie został dodany do zlecenia."
						, issueLine.ProductId, product.SKU, issueLine.Quantity, totalAvailable);
			}
			//2. Oblicz pełne palety, Przydzielanie pełnych lub/z datą palet
			var requiredFullPallets = 0;
			var palletAssigned = new List<Pallet>();
			var missingPalletsCount = 0;
			switch (policy)
			{
				case IssueAllocationPolicy.FullPalletFirst:
					requiredFullPallets = await _getNumberPalletsAndRestService.GetBackOnlyFullPallets(issueLine.ProductId, issueLine.Quantity);
					missingPalletsCount = requiredFullPallets - oldCount;
					palletAssigned = await SelectAndAssignFullPallets(issue, issueLine, reusablePalletsForProduct, requiredFullPallets, missingPalletsCount);
					break;
			
				default:
					return AssignProductToIssueResult.Fail($"Allocation policy {policy} is not supported.");
			}
			var quantityFromPallets = palletAssigned.Sum(p => p.GetProductQuantity(issueLine.ProductId));
			var rest = issueLine.Quantity - quantityFromPallets;
			if (rest < 0)
				return AssignProductToIssueResult.Fail("Allocated more product than requested.");
			//3. pobierz dostępne virtualPallet;
			var availableVirtualPalletsQuery = await _virtualPalletRepo.GetVirtualPalletsByBBAsync(issueLine.ProductId, issueLine.BestBefore);
			//4. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
			if (rest > 0)
			{
				var newPickingTaskFromRest = await _addPickingTaskToIssueService.AddPickingTaskToIssue(
					palletAssigned, availableVirtualPalletsQuery, issue,
					issueLine.ProductId, rest, issueLine.BestBefore, userId);
				if (newPickingTaskFromRest.Success is false)
				{
					return AssignProductToIssueResult.Fail(newPickingTaskFromRest.Message, issueLine.ProductId,product.SKU, issueLine.Quantity, totalAvailable);
				}
			}
			return AssignProductToIssueResult.Ok($"Towar {productSKU} został dołączony do zlecenia.",issueLine.ProductId, product.SKU, palletAssigned);
		}
		//pełne palety first
		private async Task<List<Pallet>> SelectAndAssignFullPallets(Issue issue, IssueItemDTO issueLine, List<Pallet> reusablePalletsForProduct, int requiredFullPallets, int missingPalletsCount)
		{
			List<Pallet> missingPallets = [];
			if (missingPalletsCount > 0)
			{
				var product = await _productRepo.GetProductByIdAsync(issueLine.ProductId);//checked in upper
				missingPallets = await _palletRepo.GetAvailableFullPallets(issueLine.ProductId, product!.CartonsPerPallet, issueLine.BestBefore, missingPalletsCount);
				foreach (var pallet in missingPallets)
					pallet.ChangeStatus(PalletStatus.LockedForIssue);
			}
			List<Pallet> allAvailablePallets = [.. reusablePalletsForProduct
				.Concat(missingPallets)
				.DistinctBy(p => p.Id)
				.Take(requiredFullPallets)];
			foreach (var pallet in allAvailablePallets)
			{
				var snapShot = pallet.Location.ToSnapshot();
				pallet.ReserveToIssue(issue.Id, issue.PerformedBy, snapShot);
			}
			return allAvailablePallets;
		}
		//Obecnie wspierana jest polityka FullPalletFirst; pozostałe strategie mogą zostać dodane jako osobne polityki alokacji.
	}
}
