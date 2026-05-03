using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.IssuesServices;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.CreateNewIssue
{
	public class CreateIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IAssignProductToIssueService assignProductToIssueService) : IRequestHandler<CreateIssueCommand, AppResult<List<IssueResult>>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAssignProductToIssueService _assignProductToIssueService = assignProductToIssueService;

		public async Task<AppResult<List<IssueResult>>> Handle(CreateIssueCommand request, CancellationToken ct)
		{
			var addedProducts = new List<IssueResult>();
			var results = new List<IssueResult>();
			var issueNumber = await _issueRepo.GetNextNumberOfIssue();
			var issue = Issue.Create(issueNumber, request.DTO.ClientId, request.Date, request.DTO.PerformedBy);
			_issueRepo.AddIssue(issue);
			foreach (var item in request.DTO.Items)
			{
				IssueResult addingProducts;
				var result = await _assignProductToIssueService.AssignProductToIssue(issue, item, IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy);
				if (result.Success == false)
				{
					addingProducts = IssueResult.Fail(result.Message, item.ProductId, result.QuantityRequest, result.QuantityOnStock);
				}
				else
				{
					addingProducts = IssueResult.Ok(result.Message, item.ProductId);
				}
				addedProducts.Add(addingProducts);
				issue.AddIssueItem(item.ProductId, item.Quantity, item.BestBefore);
			}
			if (addedProducts.Any(r => r.Success == false))
			{
				issue.ChangeStatus(IssueStatus.NotComplete);
			}
			issue.AddHistory(request.DTO.PerformedBy);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<List<IssueResult>>.Success(addedProducts);
		}
	}
}