using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Pallets.Queries.GetPallet;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Infrastructure.Persistence.ReadServices
{
	public class PalletReadService(WerehouseDbContext werehouseDbContext) : IPalletReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public Task<PalletDTO?> GetPalletByIdFullInfoAsync(Guid id, CancellationToken ct)
		{
			var result = _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(p => p.Id == id)
				.Select(p => new PalletDTO
				{
					PalletNumber = p.PalletNumber,
					DateReceived = p.DateReceived,
					Status = p.Status,
					LocationSnapShot =p.Location.ToSnapshot(),					
					ProductsOnPallet = p.ProductsOnPallet
					.Select(pp => new ProductOnPalletDTO
					{
						ProductId = pp.ProductId,
						ProductSKU = pp.Product.SKU,
						ProductName = pp.Product.Name,
						Quantity = pp.Quantity,
						DateAdded = pp.DateAdded,
						BestBefore = pp.BestBefore,
					}).ToList(),
					PalletHistory = p.PalletHistory
					.Select(ph => new HistoryPalletDTO
					{
						Id = ph.Id,
						PalletNumber = ph.PalletNumber,
						LocationSnapShotSource = ph.SourceLocationSnapShot,
						LocationSnapShotDestination = ph.DestinationLocationSnapShot,
						Reason = ph.Reason,
						PerformedBy = ph.PerformedBy,
						HistoryPalletDetailsDTO = ph.HistoryPalletDetails
						.Select(phd => new HistoryPalletDetailDTO
						{
							ProductId = phd.ProductId,
							QuantityChange = phd.QuantityChange,
						}).ToList(),
						MovementDate = ph.MovementDate,
					}).ToList(),
					ReceiptNumber = p.Receipt != null
					? p.Receipt.ReceiptNumber
					: null,
					IssueNumber = p.Issue != null
					? p.Issue.IssueNumber
					: null,
				})
				.FirstOrDefaultAsync(ct);
			return result;
		}

		public Task<PalletSimplyDTO?> GetPalletByPalletNumberAsync(string palletNumber, CancellationToken ct)
		{
			var pallet = _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(p => p.Status != PalletStatus.Archived)
				.Where(p => p.PalletNumber == palletNumber)
				.OrderBy(p => p.PalletNumber)
				.Select(p => new PalletSimplyDTO
				{
					Id = p.Id,
					PalletNumber = p.PalletNumber,
					Status = p.Status,
				})
				.FirstOrDefaultAsync(ct);
			return pallet;
		}

		public Task<PagedResult<PalletSimplyDTO>> GetPalletsByFilterAsync(PalletSearchFilter filter, int currentPage, int pageSize, CancellationToken ct)
		{
			var result = _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(p => p.Status != PalletStatus.Archived);

			if (!string.IsNullOrEmpty(filter.PalletNumber))
			{
				result = result.Where(p => p.PalletNumber == filter.PalletNumber);
			}
			if (filter.ProductId.HasValue)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp => pp.ProductId == filter.ProductId));
			}
			if (!string.IsNullOrWhiteSpace(filter.ProductName))
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product != null &&
				EF.Functions.Like(pp.Product.Name.ToLower(), $"%{filter.ProductName.ToLower()}%")));
			}
			if (!string.IsNullOrWhiteSpace(filter.SKU))
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product.SKU == filter.SKU));
			}
			if (filter.LocationBay > 0)
			{
				result = result.Where(p => p.Location.Bay == filter.LocationBay);
			}
			if (filter.LocationAisle > 0)
			{
				result = result.Where(p => p.Location.Aisle == filter.LocationAisle);
			}
			if (filter.LocationPosition > 0)
			{
				result = result.Where(p => p.Location.Position == filter.LocationPosition);
			}
			if (filter.LocationHeight > 0)
			{
				result = result.Where(p => p.Location.Height == filter.LocationHeight);
			}
			if (filter.PalletStatus.HasValue)
			{
				result = result.Where(p => p.Status == filter.PalletStatus);
			}
			if (filter.BestBeforeFrom != null || filter.BestBeforeTo != null)
			{
				var bestBeforeStart = filter.BestBeforeFrom ?? DateOnly.MinValue;
				var bestBeforeEnd = filter.BestBeforeTo ?? DateOnly.MaxValue;
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.BestBefore >= bestBeforeStart && pp.BestBefore <= bestBeforeEnd));
			}
			if (filter.StartDate != null)
			{
				var start = filter.StartDate.Value;
				var end = filter.EndDate ?? DateTime.Now;

				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.DateAdded >= start && pp.DateAdded <= end));
			}
			if (filter.ClientIdIn != null)
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.ClientId == filter.ClientIdIn);
			}
			if (filter.ClientIdOut != null)
			{
				result = result.Where(p => p.Issue != null && p.Issue.ClientId == filter.ClientIdOut);
			}
			if (!string.IsNullOrEmpty(filter.ReceiptUser))
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.PerformedBy == filter.ReceiptUser);
			}
			if (!string.IsNullOrEmpty(filter.IssueUser))
			{
				result = result.Where(p => p.Issue != null && p.Issue.PerformedBy == filter.IssueUser);
			}
			var resultToShow = result
				.OrderBy(p => p.PalletNumber)
				.Select(p => new PalletSimplyDTO
				{
					Id = p.Id,
					PalletNumber = p.PalletNumber,
					Status = p.Status,
				});
			return resultToShow.ToPagedResultAsync(currentPage, pageSize, ct);
		}

		public Task<ShowPalletToEditDTO?> ShowPalletToEditAsync(Guid id, CancellationToken ct)
		{
			var pallet = _werehouseDbContext.Pallets
				.AsNoTracking()
				.Where(p => p.Status != PalletStatus.Archived)
				.Where(p => p.Id == id)
				.Select(p => new ShowPalletToEditDTO
				{
					Id = p.Id,
					LocationId = p.LocationId,

					PalletNumber = p.PalletNumber,
					DateReceived = p.DateReceived,
					Status = p.Status,
					LocationSnapShot =p.Location.ToSnapshot(),					
					ProductsOnPallet = p.ProductsOnPallet
					.Select(pp => new ProductOnPalletToEditDTO
					{
						ProductId = pp.ProductId,
						ProductName = pp.Product.Name,
						SKU = pp.Product.SKU,
						DateAdded = pp.DateAdded,
						Quantity = pp.Quantity,
						BestBefore = pp.BestBefore
					}).ToList()
				})
				.FirstOrDefaultAsync(ct);
			return pallet;
		}
	}
}
