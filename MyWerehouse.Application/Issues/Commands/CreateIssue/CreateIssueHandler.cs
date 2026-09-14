using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public class CreateIssueHandler(IUnitOfWork unitOfWork,
		IIssueRepo issueRepo,
		IAssignProductToIssueService assignProductToIssueService,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateIssueCommand, AppResult<IssueCreateModifyResult>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAssignProductToIssueService _assignProductToIssueService = assignProductToIssueService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		public async Task<AppResult<IssueCreateModifyResult>> Handle(CreateIssueCommand request, CancellationToken ct)
		{
			return await _unitOfWork.ExecuteInTransactionAsync(
				async transactionCt =>
				{
					var now = _dateTimeProvider.UtcNow;
					var results = new List<AssignProductToIssueResult>();
					var issuenumber = await _issueRepo.GetNextNumberOfIssue(transactionCt);
					var issue = Issue.Create(
						issuenumber,
						request.DTO.ClientId,
						request.SendDate,
						now,
						request.DTO.PerformedBy);
					foreach (var item in request.DTO.Items)
					{
						var result = await _assignProductToIssueService.AssignGoodsToIssue(issue, item,
							IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy, transactionCt);
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
					await _unitOfWork.SaveChangesAsync(transactionCt);
					return AppResult<IssueCreateModifyResult>.Success(
						new IssueCreateModifyResult(
							issue.Id,
							issue.IssueNumber,
							"Issue was created.",
							results));
				}, IsolationLevel.Serializable, ct);
		}
	}
}
