using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public class ComparePlanToPreparedService(IIssueRepo issueRepo, IProductRepo productRepo) : IComparePlanToPreparedService
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IProductRepo _productRepo = productRepo;
		public async Task<ComparePlanToPreparedResult> ComparePlanToPrepared(Guid issueId, Guid productId)
		{
			var product = await _productRepo.GetProductByIdAsync(productId);
			if (product == null)
			{
				return ComparePlanToPreparedResult.Fail("Produkt nie nie istnieje.");
			}
			var sku = product.SKU;
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null)
			{
				return ComparePlanToPreparedResult.Fail("Brak zlecenia wydania");
			}
			var issueItemForProduct = issue.IssueItems.FirstOrDefault(p => p.ProductId == productId);
			if (issueItemForProduct == null)
			{
				return ComparePlanToPreparedResult.Fail("Produkt nie występuje w zleceniu.", productId, sku);
			}
			var dateBB = issueItemForProduct.BestBefore;
			
			var pallets = _issueRepo.GetPalletsByIssueId(issueId);
			foreach (var pallet in pallets)
			{
				if(pallet.Status != Domain.Pallets.Models.PalletStatus.ToIssue)
				{
					return ComparePlanToPreparedResult.Fail("Nie wszystkie palety do załadunku mają odpowiedni status.");
				}
			}
			var quantityFromPallets =await pallets
				.SelectMany(p => p.ProductsOnPallet)
				.Where(pp => pp.ProductId == productId && pp.BestBefore == dateBB)
				.SumAsync(pp => pp.Quantity);
			
			if (issueItemForProduct.Quantity == quantityFromPallets)
			{
				return ComparePlanToPreparedResult.Ok("Towar się zgadza.", productId, sku);
			}
			else
			{
				return ComparePlanToPreparedResult.Fail($"Towar się nie zgadza. Zażądano {issueItemForProduct.Quantity} z BB {dateBB} a przygotowano {quantityFromPallets}. Sprawdź ilość oraz daty BB na paletach.", productId, sku, issueItemForProduct.Quantity, quantityFromPallets);
			}
		}
	}
}
