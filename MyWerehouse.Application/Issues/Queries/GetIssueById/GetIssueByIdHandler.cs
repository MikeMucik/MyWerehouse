using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class GetIssueByIdHandler : IRequestHandler<GetIssueByIdQuery, AppResult<IssueDTO>>
	{
		private readonly IMapper _mapper;
		private readonly IIssueRepo _issueRepo;
		public GetIssueByIdHandler(IMapper mapper,
			IIssueRepo issueRepo)
		{
			_issueRepo = issueRepo;
			_mapper = mapper;
		}
		public async Task<AppResult<IssueDTO>> Handle(GetIssueByIdQuery request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<IssueDTO>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			//if (issue == null) { throw new NotFoundIssueException(request.IssueId); }
			var issueDTO = _mapper.Map<IssueDTO>(issue);
			return AppResult<IssueDTO>.Success(issueDTO);
			//return issueDTO;
		}
	}
}
