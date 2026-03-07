using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr
{
	public class GetIssuesByFiltrHandler(IIssueRepo issueRepo, IMapper mapper) : IRequestHandler<GetIssuesByFiltrQuery, AppResult< List<IssueDTO>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMapper _mapper = mapper;
		public async Task<AppResult< List<IssueDTO>>> Handle(GetIssuesByFiltrQuery request, CancellationToken ct)
		{
			var issues =await _issueRepo.GetIssuesByFilter(request.Filtr).ToListAsync(ct);
			var dto = _mapper.Map<List<IssueDTO>>(issues);
			return AppResult<List<IssueDTO>>.Success(dto);
		}
	}
}
