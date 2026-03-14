using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Queries.LoadingIssueList
{
	public class LoadingIssueListHandler(IIssueRepo issueRepo) : IRequestHandler<LoadingIssueListQuery, AppResult< ListPalletsToLoadDTO>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;

		public async Task<AppResult<ListPalletsToLoadDTO>> Handle (LoadingIssueListQuery request, CancellationToken ct)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
				if (issue == null)
			{
				return AppResult<ListPalletsToLoadDTO>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			//zebrać palety po wysyłki 		trzeba się zastanowić czy status tylko ToIssue					 
			var dto = new ListPalletsToLoadDTO
			{
				IssueId = issue.Id,
				IssueNumber =issue.IssueNumber,
				ClientId = issue.ClientId,
				ClientName = issue.Client.Name,
				Pallets = issue.Pallets
				.Where(p =>
				p.Status == PalletStatus.InTransit ||
				p.Status == PalletStatus.InStock ||
				p.Status == PalletStatus.Available ||
				p.Status == PalletStatus.ToIssue
				)
				.Select(p => new PalletToLoadDTO
				{
					PalletId = p.Id,
					LocationName = (p.Location.Bay + " " + p.Location.Aisle + " " + p.Location.Position + " " + p.Location.Height).ToString(),
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
			};
			return AppResult<ListPalletsToLoadDTO>.Success(dto);
		}
	}
}
