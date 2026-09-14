using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFilter;
using MyWerehouse.Application.Issues.Queries.IssueProductsSummary;
using MyWerehouse.Application.Issues.Queries.LoadingIssueList;
using MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Server.ServicesToInfrastructure
{
	public class IssueReadService(WerehouseDbContext werehouseDbContext) : IIssueReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public Task<PagedResult<IssueSimplyDTO>> GetIssueByFilter(IssueReceiptSearchFilter filter, int pageNumber, int pageSize, CancellationToken ct)
		{
			var result = _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.IssueStatus != IssueStatus.Archived);
			if (filter.IssueNumber != null && filter.IssueNumber != 0)
			{
				result = result.Where(i => i.IssueNumber == filter.IssueNumber);
			}
			if (filter.ClientId > 0)
			{
				result = result.Where(i => i.ClientId == filter.ClientId);
			}
			if (filter.ClientName != null)
			{
				result = result.Where(i => i.Client.Name == filter.ClientName);
			}
			if (filter.ProductId.HasValue)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.ProductId == filter.ProductId)));
			}
			if (filter.ProductName != null)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.Name == filter.ProductName)));
			}
			if (filter.SKU != null)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.SKU == filter.SKU)));
			}
			if (filter.CreateDateStart != null)
			{
				var start = filter.CreateDateStart;
				var end = filter.CreateDateEnd ?? DateTime.UtcNow;

				result = result.Where(i => i.IssueDateTimeCreate >= start && i.IssueDateTimeCreate <= end);
			}
			if (filter.SendDateStart != null)
			{
				var start = filter.SendDateStart;
				var end = filter.SendDateEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);

				result = result.Where(i => i.IssueDateTimeSend >= start && i.IssueDateTimeSend <= end);
			}
			if (filter.UserId != null)
			{
				result = result.Where(i => i.PerformedBy == filter.UserId);
			}
			var resultDTO = result
				.OrderBy(i => i.IssueNumber)
				.Select(i => new IssueSimplyDTO
				{
					Id = i.Id,
					IssueNumber = i.IssueNumber,
					ClientId = i.ClientId,
					IssueStatus = i.IssueStatus,
					IssueDateTimeCreate = i.IssueDateTimeCreate,
					IssueDateTimeSend = i.IssueDateTimeSend,
					PerformedBy = i.PerformedBy,
				});
			var resultToShow = resultDTO.ToPagedResultAsync(pageNumber, pageSize, ct);
			return resultToShow;
		}

		public Task<IssueDTO?> GetIssueById(Guid id, CancellationToken ct)
		{
			var issue = _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.IssueStatus != IssueStatus.Archived)
				.Where(i => i.Id == id)
				.Select(i => new IssueDTO
				{
					Id = i.Id,
					IssueNumber = i.IssueNumber,
					ClientId = i.ClientId,
					ClientName = i.Client.Name,
					IssueDateTimeCreate = i.IssueDateTimeCreate,
					IssueDateTimeSend = i.IssueDateTimeSend,
					Pallets = i.Pallets
					.Select(p => new PalletDTOIssue
					{
						Id = p.Id,
						PalletNumber = p.PalletNumber,
						LocationId = p.LocationId,
						Status = p.Status,
						LocationSnapShot = p.Location.ToSnapshot(),
						ProductsOnPallet = p.ProductsOnPallet
						.Select(pp => new ProductOnPalletDTO
						{
							ProductId = pp.ProductId,
							ProductSKU = pp.Product.SKU,
							ProductName = pp.Product.Name,
							DateAdded = pp.DateAdded,
							BestBefore = pp.BestBefore,
							Quantity = pp.Quantity,
						}).ToList()
					}).ToList(),
					PerformedBy = i.PerformedBy,
					IssueStatus = i.IssueStatus,
					IssueItems = i.IssueItems
					.Select(ii => new IssueItemViewDTO
					{
						ProductId = ii.ProductId,
						ProductSKU = ii.Product.SKU,
						ProductName = ii.Product.Name,
						Quantity = ii.Quantity,
						BestBefore = ii.BestBefore,
					}).ToList()
				})
				.FirstOrDefaultAsync(ct);
			return issue;
		}

		public Task<PagedResult<PalletWithLocationDTO>> GetPalletToTakeOff(Guid id, int pageNumber, int pageSize, CancellationToken ct)
		{
			var query = _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.Id == id && i.IssueStatus != IssueStatus.Archived)
				.SelectMany(a => a.Pallets)
					.OrderBy(l => l.Location.Bay)
						.ThenBy(l => l.Location.Aisle)
							.ThenBy(l => l.Location.Position)
								.ThenBy(l => l.Location.Height)
									.ThenBy(l => l.Id)
				.Select(p => new PalletWithLocationDTO
				{
					PalletId = p.Id,
					PalletNumber = p.PalletNumber,
					LocationId = p.LocationId,
					Status = p.Status,
					LocationName = p.Location.ToSnapshot()
				});
			var queryToShow = query.ToPagedResultAsync(pageNumber, pageSize, ct);
			return queryToShow;
		}

		public Task<ListPalletsToLoadDTO?> ListPalletsToLoad(Guid id, CancellationToken ct)
		{
			var dto = _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.Id == id)
				.Where(i => i.IssueStatus != IssueStatus.Archived)
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
			return dto;
		}

		public Task<SummaryProductsIssueDTO?> SummaryProductsIssue(Guid id, CancellationToken ct)
		{
			var dto = _werehouseDbContext.Issues
				.AsNoTracking()
				.Where(i => i.Id == id)
				.Where(i => i.IssueStatus != IssueStatus.Archived)
				.Select(x => new SummaryProductsIssueDTO
				{
					Id = x.Id,
					IssueNumber = x.IssueNumber,
					ClientId = x.ClientId,
					PerformedBy = x.PerformedBy,
					IssueItems = x.IssueItems
					 .Select(ii => new IssueItemViewDTO
					 {
						 ProductId = ii.ProductId,
						 ProductName = ii.Product.Name,
						 ProductSKU = ii.Product.SKU,
						 Quantity = ii.Quantity,
						 BestBefore = ii.BestBefore
					 }).ToList(),
					DateToSend = x.IssueDateTimeSend
				}).FirstOrDefaultAsync(ct);
			return dto;
		}
	}
}
