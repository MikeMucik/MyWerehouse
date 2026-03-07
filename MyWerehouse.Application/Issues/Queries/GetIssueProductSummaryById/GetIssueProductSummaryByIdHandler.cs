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
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class GetIssueProductSummaryByIdHandler : IRequestHandler<GetIssueProductSummaryByIdQuery, AppResult<UpdateIssueDTO>>
	{
		private readonly IIssueRepo _issueRepo;
		public GetIssueProductSummaryByIdHandler(IIssueRepo issueRepo)
		{
			_issueRepo = issueRepo;
		}
		public async Task<AppResult<UpdateIssueDTO>> Handle(GetIssueProductSummaryByIdQuery query, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(query.IssueId);// ?? throw new NotFoundIssueException(query.IssueId);
			if (issue == null) { return AppResult<UpdateIssueDTO>.Fail($"Zamówienie o numerze {query.IssueId} nie zostało znalezione.", ErrorType.NotFound); }
			var dto = new UpdateIssueDTO
			{
				Id = issue.Id,
				ClientId = issue.ClientId,
				PerformedBy = issue.PerformedBy,
				Items = issue.IssueItems
				 .Select(ii => new IssueItemDTO
				 {
					 ProductId = ii.ProductId,
					 Quantity = ii.Quantity
				 }).ToList(),
				DateToSend = issue.IssueDateTimeSend
			};
			return AppResult<UpdateIssueDTO>.Success(dto);
		}
	}
}