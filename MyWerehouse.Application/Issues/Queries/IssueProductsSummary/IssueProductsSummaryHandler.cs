using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Queries.IssueProductsSummary
{
	public class IssueProductsSummaryHandler(WerehouseDbContext werehouseDbContext) : IRequestHandler<IssueProductsSummaryQuery, AppResult<SummaryProductsIssueDTO>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<SummaryProductsIssueDTO>> Handle(IssueProductsSummaryQuery query, CancellationToken ct)
		{
			var dto = await _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.Id == query.IssueId)
				.Select(x => new SummaryProductsIssueDTO
				{
					Id = x.Id,
					ClientId = x.ClientId,
					PerformedBy = x.PerformedBy,
					IssueItems = x.IssueItems
					 .Select(ii => new IssueItemDTO
					 {
						 ProductId = ii.ProductId,
						 ProductName = ii.Product.Name,
						 ProductSKU = ii.Product.SKU,
						 Quantity = ii.Quantity,
						 BestBefore = ii.BestBefore
					 }).ToList(),
						DateToSend = x.IssueDateTimeSend
				}).FirstOrDefaultAsync(ct);
			
			if (dto == null)
			{
				return AppResult<SummaryProductsIssueDTO>.Fail($"Zamówienie o numerze {query.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			return AppResult<SummaryProductsIssueDTO>.Success(dto);
		}
	}
}