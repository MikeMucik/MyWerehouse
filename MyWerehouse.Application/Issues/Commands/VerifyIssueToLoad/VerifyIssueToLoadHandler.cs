using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public class VerifyIssueToLoadHandler(
		IIssueRepo issueRepo,
		IProductRepo productRepo,
		IUnitOfWork unitOfWork) : IRequestHandler<VerifyIssueToLoadCommand, AppResult<List<ComparePlanToPreparedResult>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;

		public async Task<AppResult<List<ComparePlanToPreparedResult>>> Handle(VerifyIssueToLoadCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId, ct);
			if (issue == null)
				return AppResult<List<ComparePlanToPreparedResult>>.Fail("Issue was not found.");
			//check requested amount = prepered amount
			var listOfProduct = issue.IssueItems.Select(x => x.ProductId);
			var resultComparing = new List<ComparePlanToPreparedResult>();
			bool isConditional = false;
			foreach (var productId in listOfProduct)
			{
				var product = await _productRepo.GetProductByIdAsync(productId, ct);
				if (product == null)
				{
					return AppResult<List<ComparePlanToPreparedResult>>.Fail("Product does not exist.");
				}				
				var issueVerifyResult = issue.CompareGoods(productId);
				if (!issueVerifyResult.IsMatching && issueVerifyResult.IsConditional)
				{
					resultComparing.Add(ComparePlanToPreparedResult.Ok(
						"Prepared product conditionally added to the issue.",
						productId,
						product.SKU,
						issueVerifyResult.OrderedQuantity,
						issueVerifyResult.PreparedQuantity));
					isConditional = true;
				}
				else if (issueVerifyResult.IsMatching)
				{
					resultComparing.Add(ComparePlanToPreparedResult.Ok(
						"Prepared product matches the issue.",
						productId,
						product.SKU,
						issueVerifyResult.OrderedQuantity,
						issueVerifyResult.PreparedQuantity));
				}
				else
				{
					resultComparing.Add(ComparePlanToPreparedResult.Fail($"Prepared product does not match the issue. Requested {issueVerifyResult.OrderedQuantity} with best-before date {issueVerifyResult.BestBefore}, but prepared {issueVerifyResult.PreparedQuantity}. Check pallet quantities and best-before dates.", productId, product.SKU, issueVerifyResult.OrderedQuantity, issueVerifyResult.PreparedQuantity));
				}
			}
			if (resultComparing.Any(a => a.Success == false))
			{
				return AppResult<List<ComparePlanToPreparedResult>>.Fail("Issue was not approved.", resultComparing, ErrorType.Validation);
			}
			issue.VerifyToLoad(request.UserId);
			await _unitOfWork.SaveChangesAsync(ct);
			string message;
			if (isConditional)
			{
				message = "Issue was conditionally approved with incomplete picking.";
			}
			else
			{
				message = "Issue approved.";
			};
			return AppResult<List<ComparePlanToPreparedResult>>.Success(resultComparing, message);
		}
	}
}
