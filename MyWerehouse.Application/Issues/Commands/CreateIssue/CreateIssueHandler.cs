using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public class CreateIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IAssignProductToIssueService assignProductToIssueService,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateIssueCommand, AppResult<IssueCreateModifyResult>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAssignProductToIssueService _assignProductToIssueService = assignProductToIssueService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		public async Task<AppResult<IssueCreateModifyResult>> Handle(CreateIssueCommand request, CancellationToken ct)
		{
			var strategy = _werehouseDbContext.Database.CreateExecutionStrategy();
			return await strategy.ExecuteAsync(async () =>
			{
				_werehouseDbContext.ChangeTracker.Clear();
				await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(
				IsolationLevel.Serializable, ct);
				var now = _dateTimeProvider.UtcNow;
				var results = new List<AssignProductToIssueResult>();
				var issueNumber = await _issueRepo.GetNextNumberOfIssue(ct);
				var issue = Issue.Create(issueNumber, request.DTO.ClientId,
					request.SendDate, now, request.DTO.PerformedBy);
				foreach (var item in request.DTO.Items)
				{
					var result = await _assignProductToIssueService.AssignGoodsToIssue(issue, item,
						IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy, ct);
					if (result.Success != false)
					{
						issue.AddIssueItem(item.ProductId, item.Quantity, item.BestBefore, now);
					}
					results.Add(result);
				}
				if (results.Any(r => r.Success == false))
				{
					issue.ChangeStatus(IssueStatus.RequiresCorrection);
				}
				_issueRepo.AddIssue(issue);
				issue.AddHistory(request.DTO.PerformedBy);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);

				var response = IssueCreateModifyResult.Ok(
					issue.Id,
					issue.IssueNumber,
					"Issue was created.",
					results);

				return AppResult<IssueCreateModifyResult>.Success(response);
			});
		}
	}
}
