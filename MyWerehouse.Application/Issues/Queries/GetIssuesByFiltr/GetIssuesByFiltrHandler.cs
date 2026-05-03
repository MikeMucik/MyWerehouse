using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr
{
	public class GetIssuesByFiltrHandler(IIssueRepo issueRepo, IMapper mapper) : IRequestHandler<GetIssuesByFiltrQuery, AppResult<PagedResult<IssueDTO>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMapper _mapper = mapper;
		public async Task<AppResult<PagedResult<IssueDTO>>> Handle(GetIssuesByFiltrQuery request, CancellationToken ct)
		{
			var issues = _issueRepo.GetIssuesByFilter(request.Filtr);
			var issueOrdered = issues.OrderBy(i => i.Id);
			var result = await issueOrdered.ToPagedResultAsync<Issue, IssueDTO>(
				_mapper.ConfigurationProvider,
				request.CurrentPage,
				request.PageSize,
				ct);
			
			if (result.TotalCount == 0) return AppResult<PagedResult<IssueDTO>>.Fail("Brak zleceń o zadanych parametrach", ErrorType.NotFound);

			return AppResult<PagedResult<IssueDTO>>.Success(result);
		}
	}
}
