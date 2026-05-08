using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletsToTakeOffListHandler(IIssueRepo issueRepo
		, ILocationRepo locationRepo) : IRequestHandler<PalletsToTakeOffListQuery, AppResult<PagedResult<PalletWithLocationDTO>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		public async Task<AppResult<PagedResult<PalletWithLocationDTO>>> Handle(PalletsToTakeOffListQuery request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
			{
				return AppResult<PagedResult<PalletWithLocationDTO>>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			if (issue.IssueStatus != IssueStatus.ConfirmedToLoad)
			{
				return AppResult<PagedResult<PalletWithLocationDTO>>.Fail($"Zamówienie o numerze {issue.IssueNumber} nie zostało zatwierdzone do załadunku", ErrorType.NotFound);
			}
			var query = _issueRepo.GetPalletsByIssueId(request.IssueId)
				.OrderBy(l => l.Location)
				.Select((p => new PalletWithLocationDTO
				{
					PalletId = p.Id,
					PalletNumber = p.PalletNumber,
					LocationId = p.LocationId,
					LocationName =
					p.Location.Bay + "-" +
					p.Location.Aisle + "-" +
					p.Location.Position + "-" +
					p.Location.Height
				}));
			var result = await query
				.ToPagedResultAsync(request.PageNumber, request.PageSize, ct);

			return AppResult<PagedResult<PalletWithLocationDTO>>.Success(result);
		}
	}
}
