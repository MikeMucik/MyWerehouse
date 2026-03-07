using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletsToTakeOffListHandler(IIssueRepo issueRepo) : IRequestHandler<PalletsToTakeOffListQuery, AppResult<IssuePalletsWithLocationDTO>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		public async Task<AppResult<IssuePalletsWithLocationDTO>> Handle(PalletsToTakeOffListQuery request, CancellationToken ct)
		{
			var list = await _issueRepo.GetPalletByIssueIdAsync(request.IssueId);
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if(issue == null)
			{
				return AppResult<IssuePalletsWithLocationDTO>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			if (issue.IssueStatus != IssueStatus.ConfirmedToLoad)
			{
				return AppResult<IssuePalletsWithLocationDTO>.Fail($"Zamówienie o numerze {issue.IssueNumber} nie zostało zatwierdzone do załadunku", ErrorType.NotFound);
			}
			var listToShow = new IssuePalletsWithLocationDTO
			{
				IssueNumber = issue.IssueNumber,
				PalletList = list
			};
			return AppResult<IssuePalletsWithLocationDTO>.Success(listToShow);
		}
	}

}
