using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public class VerifyIssueToLoadHandler(
		IIssueRepo issueRepo,
		IProductRepo productRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<VerifyIssueToLoadCommand, AppResult<List<ComparePlanToPreparedResult>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<List<ComparePlanToPreparedResult>>> Handle(VerifyIssueToLoadCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<List<ComparePlanToPreparedResult>>.Fail("Issue was not found.");
			//check requested amount = prepered amount
			var listOfProduct = issue.IssueItems.Select(x => x.ProductId);
			var resultComparing = new List<ComparePlanToPreparedResult>();
			var isConditional = issue.IssueStatus == IssueStatus.PickingShortage;
			if (!isConditional)
			{
				issue.CheckPalletsInIssue();
			}
			foreach (var productId in listOfProduct)
			{
				var product = await _productRepo.GetProductByIdAsync(productId);
				if (product == null)
				{
					return AppResult<List<ComparePlanToPreparedResult>>.Fail("Product does not exist.");
				}
				var (isMatching, preparedQuantity, orderedQuantity, bestBefore) = issue.CompareGoods(productId);
				if (isMatching)
				{
					resultComparing.Add(ComparePlanToPreparedResult.Ok("Prepared product matches the issue.", productId, product.SKU));
				}
				else if (isConditional && preparedQuantity < orderedQuantity)
				{
					resultComparing.Add(ComparePlanToPreparedResult.Ok($"Prepared product conditionally added to the issue.", productId, product.SKU));
				}
				else
				{
					resultComparing.Add(ComparePlanToPreparedResult.Fail($"Prepared product does not match the issue. Requested {orderedQuantity} with best-before date {bestBefore}, but prepared {preparedQuantity}. Check pallet quantities and best-before dates.", productId, product.SKU, orderedQuantity, preparedQuantity));
				}
			}
			if (resultComparing.Any(a => a.Success == false))
			{
				return AppResult<List<ComparePlanToPreparedResult>>.Fail("Issue was not approved.", resultComparing, ErrorType.Validation);
			}
			issue.VerifyToLoad(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			var message = isConditional ? "Issue was conditionally approved with incomplete picking." : "Issue approved.";
			return AppResult<List<ComparePlanToPreparedResult>>.Success(resultComparing, message);
		}
	}
}
