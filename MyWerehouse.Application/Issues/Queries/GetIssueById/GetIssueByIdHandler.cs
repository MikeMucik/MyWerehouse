using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class GetIssueByIdHandler :IRequestHandler<GetIssueByIdQuery, IssueDTO>
	{
		private readonly IMapper _mapper;
		private readonly IIssueRepo _issueRepo;
		public GetIssueByIdHandler(IMapper mapper,
			IIssueRepo issueRepo)
		{
			_issueRepo = issueRepo;
			_mapper = mapper;
		}
		public async Task<IssueDTO> Handle(GetIssueByIdQuery request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null) {throw new IssueException(request.IssueId); }
			var issueDTO = _mapper.Map<IssueDTO>(issue);
			return issueDTO;
		}
	}
}
