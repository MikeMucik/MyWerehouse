using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class GetIssueProductSummaryByIdHandler : IRequestHandler<GetIssueProductSummaryByIdQuery, UpdateIssueDTO>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IMapper _mapper;
		public GetIssueProductSummaryByIdHandler(IIssueRepo issueRepo,
			IMapper mapper)
		{
			_issueRepo = issueRepo;
			_mapper = mapper;
		}
		public async Task<UpdateIssueDTO> Handle(GetIssueProductSummaryByIdQuery query, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(query.IssueId) ?? throw new NotFoundIssueException(query.IssueId);
			
			return new UpdateIssueDTO
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
		}
	}
}
//var items = _mapper.Map<IssueItemDTO>(issue.IssueItems);
 //var items = issue.IssueItems
 //	//.OrderBy(a=>a.ProductId)
 //	.ProjectTo<IssueItemDTO>(_mapper.ConfigurationProvider)
 //	.ToListAsync();

//var items = issue.Pallets
//	.SelectMany(p => p.ProductsOnPallet)
//	.GroupBy(pp => pp.ProductId)
//	.Select(g => new IssueItemDTO
//	{
//		ProductId = g.Key,
//		Quantity = g.Sum(x => x.Quantity) // suma z palet -> odpowiada ilości w Issue
//	})
//	.ToList();