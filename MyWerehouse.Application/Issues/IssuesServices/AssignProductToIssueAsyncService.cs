using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pallets.Services;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssuesServices
{
	public class AssignProductToIssueAsyncService(
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IGetVirtualPalletsService getVirtualPalletsService,
		IGetProductCountService getProductCountService,
		IGetNumberPalletsAndRestService getNumberPalletsAndRestService,
		IGetAvailablePalletsByProductService getAvailablePalletsByProductService,		
		IProductRepo productRepo) : IAssignProductToIssueService
	{
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IGetVirtualPalletsService _getVirtualPalletsService = getVirtualPalletsService;
		private readonly IGetProductCountService _getProductCountService = getProductCountService;
		private readonly IGetNumberPalletsAndRestService _getNumberPalletsAndRestService = getNumberPalletsAndRestService;
		private readonly IGetAvailablePalletsByProductService _getAvailablePalletsByProductService = getAvailablePalletsByProductService;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<AssignProductToIssueResult> AssignProductToIssue(Issue issue, IssueItemDTO product, IssueAllocationPolicy policy, IReadOnlyCollection<Pallet> alreadyAssignedPallets, string userId)
		{
			if (issue.IssueStatus == IssueStatus.New) issue.IssueStatus = IssueStatus.Pending;
			if (issue.IssueStatus != IssueStatus.Pending && issue.IssueStatus != IssueStatus.New &&
			issue.IssueStatus != IssueStatus.NotComplete)
			{
				return AssignProductToIssueResult.Fail("Błąd statusu zlecenia");
			}
			if (!await _productRepo.IsExistProduct(product.ProductId))
				return AssignProductToIssueResult.Fail($"Produkt o numerze {product.ProductId} nie istnieje.");
			IReadOnlyCollection<Pallet> oldProperPallets = [];
			if (alreadyAssignedPallets != null)
			{
				 oldProperPallets = alreadyAssignedPallets.Where(p => p.ProductsOnPallet.First().ProductId == product.ProductId).ToList();
			}
			alreadyAssignedPallets ??= [];
			var oldCount = oldProperPallets.Count;
			var productToAdded = await _productRepo.GetProductByIdAsync(product.ProductId);
			if (productToAdded is null) return AssignProductToIssueResult.Fail($"Produkt o numerze {product.ProductId} nie istnieje.");
			
			//1. dostępność towaru	
			var totalAvailable = await _getProductCountService.GetProductCountAsync(product.ProductId, product.BestBefore);
			if (product.Quantity > totalAvailable)//
			{
				return AssignProductToIssueResult.Fail($"Nie wystarczająca ilości produktu o numerze {product.ProductId}. Asortyment nie został dodany do zlecenia."
						, product.ProductId, product.Quantity, totalAvailable);
			}			 
			
			//2. Oblicz pełne palety, Przydzielanie pełnych lub z datą palet
			var amountPallets = 0;
			var palletAssigned = new List<Pallet>();
			switch (policy)
			{
				case IssueAllocationPolicy.FullPalletFirst:

					amountPallets = await _getNumberPalletsAndRestService.GetBackOnlyFullPallets(product.ProductId, product.Quantity);
					palletAssigned = await SelectAndAssaignFullPallets(issue, product, alreadyAssignedPallets, amountPallets);
					break;
				//case IssueAllocationPolicy.FefoWithFullPalletPreference:

				default:
					return AssignProductToIssueResult.Fail($"Allocation policy {policy} is not supported.");				
			}

			var quantityFromPallets = palletAssigned.Sum(q => q.ProductsOnPallet.First().Quantity);

			var rest = product.Quantity - quantityFromPallets;
			if (rest < 0)
				return AssignProductToIssueResult.Fail("Allocated more product than requested.");
			//3. pobierz dostępne virtualPallet;
			var availableVirtualPalletsQuery = await _getVirtualPalletsService.GetVirtualPalletsAsync(product.ProductId, product.BestBefore);
			
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
		private async Task<List<Pallet>> SelectAndAssaignFullPallets(Issue issue, IssueItemDTO product, IReadOnlyCollection<Pallet> alreadyAssignedPallets, int amount)
		{
			var availablePallets = await _getAvailablePalletsByProductService.GetPallets(product.ProductId, product.BestBefore, amount);

			List<Pallet> allAvailablePallets = [.. alreadyAssignedPallets
				.Concat(availablePallets)
				.DistinctBy(p => p.Id)
				.Take(amount)];			
			foreach (var pallet in allAvailablePallets)
			{
				issue.AssignPallet(pallet, issue.PerformedBy);				
			}
			return allAvailablePallets;
		}
	}
}
