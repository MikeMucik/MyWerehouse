using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public class VerifyIssueToLoadHandler(IIssueRepo issueRepo, IComparePlanToPreparedService comparePlanToPreparedService,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<VerifyIssueToLoadCommand, AppResult<List<ComparePlanToPreparedResult>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IComparePlanToPreparedService _comparePlanToPreparedService = comparePlanToPreparedService;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<List<ComparePlanToPreparedResult>>> Handle(VerifyIssueToLoadCommand request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<List<ComparePlanToPreparedResult>>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			//check requested amount = prepered amount
			
			var listOfProduct = issue.IssueItems.Select(x => x.ProductId);
			var resultComparing = new List<ComparePlanToPreparedResult>();	
			foreach (var product in listOfProduct)
			{
				var result = await _comparePlanToPreparedService.ComparePlanToPrepared(request.IssueId, product);
				resultComparing.Add(result);
			}
			if (issue.IssueStatus == Domain.Issuing.Models.IssueStatus.PickingShortage)
			{
				issue.VerifyToLoad(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(ct);
				return AppResult<List<ComparePlanToPreparedResult>>.Success(resultComparing, "Wydanie zatwierdzono warunkowo z nieskończoną kompletacją.");
			}
			if (resultComparing.Any(a=>a.Success == false))
			{
				return AppResult<List<ComparePlanToPreparedResult>>.Fail("Wydania nie zatwierdzono.",resultComparing, ErrorType.Validation);
			}
			issue.VerifyToLoad(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<List<ComparePlanToPreparedResult>>.Success(resultComparing, "Wydanie zatwierdzono.");
		}
	}
}
