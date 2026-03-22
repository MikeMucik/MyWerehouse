using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletsToTakeOffListHandler(IIssueRepo issueRepo
		,ILocationRepo locationRepo) : IRequestHandler<PalletsToTakeOffListQuery, AppResult<IssuePalletsWithLocationDTO>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		public async Task<AppResult<IssuePalletsWithLocationDTO>> Handle(PalletsToTakeOffListQuery request, CancellationToken ct)
		{
			var list = await _issueRepo.GetPalletByIssueIdAsync(request.IssueId);
			var listDTO = new List<PalletWithLocationDTO>();
			foreach (var item in list)
			{
				var location =await _locationRepo.GetLocationByIdAsync(item.LocationId);
				var row = new PalletWithLocationDTO
				{
					PalletId = item.PalletId,
					PalletNumber = item.PalletNumber,
					LocationId = item.LocationId,
					LocationName = location.ToSnopShot()
				};
				listDTO.Add(row);
			}
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
				PalletList = listDTO
			};
			return AppResult<IssuePalletsWithLocationDTO>.Success(listToShow);
		}
	}

}
