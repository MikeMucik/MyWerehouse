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
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFilter
{
	public class GetIssuesByFilterHandler(IIssueRepo issueRepo, IMapper mapper) 
		: IRequestHandler<GetIssuesByFilterQuery, AppResult<PagedResult<IssueSimplyDTO>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMapper _mapper = mapper;
		public async Task<AppResult<PagedResult<IssueSimplyDTO>>> Handle(GetIssuesByFilterQuery request, CancellationToken ct)
		{
			var issues = _issueRepo.GetIssuesByFilter(request.Filter)
				.AsNoTracking();
			var issueOrdered = issues.OrderBy(i => i.Id);
			var result = await issueOrdered
				.ProjectTo<IssueSimplyDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(request.CurrentPage,request.PageSize,ct);			
			if (result.TotalCount == 0) return AppResult<PagedResult<IssueSimplyDTO>>.Fail("Brak zleceń o zadanych parametrach", ErrorType.NotFound);
			return AppResult<PagedResult<IssueSimplyDTO>>.Success(result);
		}
	}
}