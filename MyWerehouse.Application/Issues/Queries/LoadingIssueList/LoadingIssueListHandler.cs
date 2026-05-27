using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Queries.LoadingIssueList
{
	public class LoadingIssueListHandler(WerehouseDbContext werehouseDbContext) : IRequestHandler<LoadingIssueListQuery, AppResult<ListPalletsToLoadDTO>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<ListPalletsToLoadDTO>> Handle(LoadingIssueListQuery request, CancellationToken ct)
		{
			var dto = await _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.Id == request.IssueId)
				.Select(x => new ListPalletsToLoadDTO
				{
					IssueId = x.Id,
					IssueNumber = x.IssueNumber,
					ClientId = x.ClientId,
					ClientName = x.Client.Name,
					Pallets = x.Pallets
				.Where(p =>
				p.Status == PalletStatus.LockedForIssue ||
				p.Status == PalletStatus.InStock ||
				p.Status == PalletStatus.Available ||
				p.Status == PalletStatus.ToIssue
				)
				.Select(p => new PalletToLoadDTO
				{
					PalletId = p.Id,
					PalletNumber = p.PalletNumber,
					LocationName = p.Location.ToSnapshot(),
					PalletStatus = p.Status,
					LocationId = p.LocationId,
					ProductOnPalletIssue = p.ProductsOnPallet.Select(pp => new ProductOnPalletIssueDTO
					{
						ProductId = pp.ProductId,
						ProductName = pp.Product.Name,
						SKU = pp.Product.SKU,
						BestBefore = pp.BestBefore,
						Quantity = pp.Quantity,
					}).ToList()
				}).OrderBy(p => p.LocationId)
				.ToList()
				}).FirstOrDefaultAsync(ct);
			if (dto == null)
			{
				return AppResult<ListPalletsToLoadDTO>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			return AppResult<ListPalletsToLoadDTO>.Success(dto);
		}
	}
}
