using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletsToTakeOffListHandler(IIssueRepo issueRepo) : IRequestHandler<PalletsToTakeOffListQuery, IssuePalletsWithLocationDTO>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		public async Task<IssuePalletsWithLocationDTO> Handle(PalletsToTakeOffListQuery request, CancellationToken ct)
		{
			var list = await _issueRepo.GetPalletByIssueIdAsync(request.IssueId);
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
				??	throw new NotFoundIssueException(request.IssueId);
			if (issue.IssueStatus != IssueStatus.ConfirmedToLoad)
				throw new NotFoundIssueException(request.IssueId);
			var listToShow = new IssuePalletsWithLocationDTO
			{
				IssueId = request.IssueId,
				PalletList = list
			};
			return listToShow;
		}
	}

}
