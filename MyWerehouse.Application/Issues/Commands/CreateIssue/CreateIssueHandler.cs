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
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public class CreateIssueHandler(WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IAssignProductToIssueService assignProductToIssueService,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateIssueCommand, AppResult<List<AssignProductToIssueResult>>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAssignProductToIssueService _assignProductToIssueService = assignProductToIssueService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		public async Task<AppResult<List<AssignProductToIssueResult>>> Handle(CreateIssueCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(
			IsolationLevel.Serializable, ct);
			var now = _dateTimeProvider.UtcNow;
			var addedProducts = new List<AssignProductToIssueResult>();
			var issueNumber = await _issueRepo.GetNextNumberOfIssue();
			var issue = Issue.Create(issueNumber, request.DTO.ClientId, request.SendDate, now, request.DTO.PerformedBy);

			foreach (var item in request.DTO.Items)
			{
				var savePoint = $"BeforeProduct_{item.ProductId}_{Guid.NewGuid}";
				await transaction.CreateSavepointAsync(savePoint, ct);
				try
				{
					var result = await _assignProductToIssueService.AssignGoodsToIssue(issue, item,
						IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy);
					if (result.Success != false)
					{
						issue.AddIssueItem(item.ProductId, item.Quantity, item.BestBefore, now);
					}
					addedProducts.Add(result);
				}
				catch (DomainException ex)
				{
					await transaction.RollbackToSavepointAsync(savePoint, ct);
					await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
					await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
					await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);

					addedProducts.Add(AssignProductToIssueResult.Fail($"An error occurred: {ex.Message}", item.ProductId));
				}
			}
			if (addedProducts.Any(r => r.Success == false))
			{
				issue.ChangeStatus(IssueStatus.RequiresCorrection);
			}
			_issueRepo.AddIssue(issue);
			issue.AddHistory(request.DTO.PerformedBy);
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			return AppResult<List<AssignProductToIssueResult>>.Success(addedProducts);
		}
	}
}
