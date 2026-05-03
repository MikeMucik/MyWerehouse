using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssuesServices
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
		IPalletRepo _palletRepo = palletRepo;

		public async Task<AssignProductToIssueResult> AssignProductToIssue(Issue issue, IssueItemDTO product, IssueAllocationPolicy policy,
			List<Pallet> reusablePalletsForProduct, string userId)
		{
			if (issue.IssueStatus == IssueStatus.New)
				issue.ChangeStatus(IssueStatus.Pending);
			if (issue.IssueStatus != IssueStatus.Pending && issue.IssueStatus != IssueStatus.New &&
			issue.IssueStatus != IssueStatus.NotComplete)
			{
				return AssignProductToIssueResult.Fail("Błąd statusu zlecenia");
			}
			if (!await _productRepo.IsExistProduct(product.ProductId))
				return AssignProductToIssueResult.Fail($"Produkt o numerze {product.ProductId} nie istnieje.");
			reusablePalletsForProduct ??= [];//zabezpieczenie null
			var oldCount = reusablePalletsForProduct.Count();
			var productToAdded = await _productRepo.GetProductByIdAsync(product.ProductId);
			if (productToAdded is null) return AssignProductToIssueResult.Fail($"Produkt o numerze {product.ProductId} nie istnieje.");
			//1. dostępność towaru	- walidacja
			var totalAvailable = await _getProductCountService.GetProductCountAsync(product.ProductId, product.BestBefore);
			if (product.Quantity > totalAvailable)//
			{
				return AssignProductToIssueResult.Fail($"Nie wystarczająca ilości produktu o numerze {product.ProductId}. Asortyment nie został dodany do zlecenia."
						, product.ProductId, product.Quantity, totalAvailable);
			}
			//2. Oblicz pełne palety, Przydzielanie pełnych lub/z datą palet
			var requiredFullPallets = 0;
			var palletAssigned = new List<Pallet>();
			var missingPalletsCount = 0;
			switch (policy)
			{
				case IssueAllocationPolicy.FullPalletFirst:
					requiredFullPallets = await _getNumberPalletsAndRestService.GetBackOnlyFullPallets(product.ProductId, product.Quantity);
					missingPalletsCount = requiredFullPallets - oldCount;					
					palletAssigned = await SelectAndAssignFullPallets(issue, product, reusablePalletsForProduct, requiredFullPallets, missingPalletsCount);
					break;
				//case IssueAllocationPolicy.FefoWithFullPalletPreference:

				default:
					return AssignProductToIssueResult.Fail($"Allocation policy {policy} is not supported.");
			}
			var quantityFromPallets = palletAssigned.Sum(p => p.GetProductQuantity(product.ProductId));
			var rest = product.Quantity - quantityFromPallets;
			if (rest < 0)
				return AssignProductToIssueResult.Fail("Allocated more product than requested.");
			//3. pobierz dostępne virtualPallet;
			var availableVirtualPalletsQuery = await _virtualPalletRepo.GetVirtualPalletsByBBAsync(product.ProductId, product.BestBefore);
			//4. Stworzenie zadania picking dla resztówki jeśli rest > 0 -  making picking for rest
			if (rest > 0)
			{
				var newPickingTaskFromRest = await _addPickingTaskToIssueService.AddPickingTaskToIssue(
					palletAssigned, availableVirtualPalletsQuery, issue,
					product.ProductId, rest, product.BestBefore, userId);
				if (newPickingTaskFromRest.Success is false)
				{
					return AssignProductToIssueResult.Fail(newPickingTaskFromRest.Message, product.ProductId, product.Quantity, totalAvailable);
				}
			}
			return AssignProductToIssueResult.Ok($"Towar {product.ProductId} został dołączony do zlecenia.", palletAssigned);
		}
		//pełne palety first
		private async Task<List<Pallet>> SelectAndAssignFullPallets(Issue issue, IssueItemDTO product, List<Pallet> reusablePalletsForProduct, int requiredFullPallets, int missingPalletsCount)
		{
			List<Pallet> availablePallets = [];
			if (missingPalletsCount > 0)
			{
				var productFullQuantity= await _productRepo.GetProductByIdAsync(product.ProductId);
				//availablePallets = await _getAvailablePalletsByProductService.GetPallets(product.ProductId, product.BestBefore, missingPalletsCount);
				availablePallets = await _palletRepo.GetAvailableFullPallets (product.ProductId, productFullQuantity.CartonsPerPallet, product.BestBefore, missingPalletsCount);
				foreach (var pallet in availablePallets) 
					pallet.ChangeStatus(PalletStatus.LockedForIssue);
			}
			List<Pallet> allAvailablePallets = [.. reusablePalletsForProduct
				.Concat(availablePallets)
				.DistinctBy(p => p.Id)
				.Take(requiredFullPallets)];
			foreach (var pallet in allAvailablePallets)
			{
				var snapShot = pallet.Location.ToSnapshot();
				pallet.ReserveToIssue(issue.Id, issue.PerformedBy, snapShot);
			}
			return allAvailablePallets;
		}
		//TODO
		//Fifo, fefo etc.
	}
}
