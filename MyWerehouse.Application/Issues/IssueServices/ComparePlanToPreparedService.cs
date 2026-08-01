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
				return ComparePlanToPreparedResult.Fail("Product does not exist.");
			}
			var sku = product.SKU;
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null)
			{
				return ComparePlanToPreparedResult.Fail("Issue was not found.");
			}
			var issueItemForProduct = issue.IssueItems.FirstOrDefault(p => p.ProductId == productId);
			if (issueItemForProduct == null)
			{
				return ComparePlanToPreparedResult.Fail("Product is not included in the issue.", productId, sku);
			}
			var dateBB = issueItemForProduct.BestBefore;

			var pallets = _issueRepo.GetPalletsByIssueId(issueId);
			foreach (var pallet in pallets)
			{
				if (pallet.Status != Domain.Pallets.Models.PalletStatus.ToIssue)
				{
					return ComparePlanToPreparedResult.Fail("Not all pallets to be loaded have the required status.");
				}
			}
			var quantityFromPallets = await pallets
				.SelectMany(p => p.ProductsOnPallet)
				.Where(pp => pp.ProductId == productId &&(dateBB == null || pp.BestBefore >= dateBB))
				.SumAsync(pp => pp.Quantity);

			if (issueItemForProduct.Quantity == quantityFromPallets)
			{
				return ComparePlanToPreparedResult.Ok("Prepared product matches the issue.", productId, sku);
			}
			else
			{
				return ComparePlanToPreparedResult.Fail($"Prepared product does not match the issue. Requested {issueItemForProduct.Quantity} with best-before date {dateBB}, but prepared {quantityFromPallets}. Check pallet quantities and best-before dates.", productId, sku, issueItemForProduct.Quantity, quantityFromPallets);
			}
		}
	}
}
