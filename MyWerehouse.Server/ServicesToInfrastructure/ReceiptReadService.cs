using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Queries.GetReceiptById;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Server.ServicesToInfrastructure
{
	public class ReceiptReadService(WerehouseDbContext werehouseDbContext) : IReceiptReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		public Task<ReceiptDTO?> GetReceiptById(Guid id, CancellationToken ct)
		{
			var receipt = _werehouseDbContext.Receipts
				.AsNoTracking()
				.Where(r => r.Id == id)
				.Where(r => r.ReceiptStatus != ReceiptStatus.Cancelled && r.ReceiptStatus != ReceiptStatus.Deleted)
				.Select(r => new ReceiptDTO
				{
					ReceiptId = r.Id,
					ReceiptNumber = r.ReceiptNumber,
					ClientId = r.ClientId,
					ClientName = r.Client.Name,
					ReceiptDateTime = r.ReceiptDateTime,
					Pallets = r.Pallets
					.Select(p => new PalletForReceiptViewDTO
					{
						Id = p.Id,
						PalletNumber = p.PalletNumber,
						DateReceived = p.DateReceived,
						LocationId = p.LocationId,
						Status = p.Status,
						ProductsOnPallet = p.ProductsOnPallet
						.Select(pp=> new ProductOnPalletDTO
						{
							ProductId = pp.ProductId,
							ProductName = pp.Product.Name,
							DateAdded = pp.DateAdded,
							ProductSKU = pp.Product.SKU,
							BestBefore = pp.BestBefore,
							Quantity = pp.Quantity
						}).ToList(),
					}).ToList(),
					PerformedBy = r.PerformedBy,
					RampNumber = r.RampNumber,
					ReceiptStatus = r.ReceiptStatus,
				})
				.FirstOrDefaultAsync(ct);
			return receipt;
		}

		public Task<PagedResult<ReceiptSimplyDTO>> GetReceiptsByFilter(
			IssueReceiptSearchFilter filter,
			int pageNumber,
			int pageSize,
			CancellationToken ct)
		{
			var receipts = _werehouseDbContext.Receipts
				.AsNoTracking()
				.Where(r => r.ReceiptStatus != ReceiptStatus.Cancelled && r.ReceiptStatus != ReceiptStatus.Deleted);
			if (filter.ReceiptNumber != null && filter.ReceiptNumber != 0)
			{
				receipts = receipts.Where(i => i.ReceiptNumber == filter.ReceiptNumber);
			}
			if (filter.ClientId > 0)
			{
				receipts = receipts.Where(i => i.ClientId == filter.ClientId);
			}
			if (filter.ClientName != null)
			{
				receipts = receipts.Where(i => i.Client.Name == filter.ClientName);
			}
			if (filter.ProductId.HasValue)
			{
				receipts = receipts.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.ProductId == filter.ProductId)));
			}
			if (filter.ProductName != null)
			{
				receipts = receipts.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.Name == filter.ProductName)));
			}
			if (filter.SKU !=  null)
			{
				receipts = receipts.Where(i=>i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.SKU == filter.SKU)));
			}
			if (filter.CreateDateStart != null)
			{
				var start = filter.CreateDateStart;
				var end = filter.CreateDateEnd ?? DateTime.UtcNow;

				receipts = receipts.Where(i => i.ReceiptDateTime >= start && i.ReceiptDateTime <= end);
			}
			if (filter.UserId != null)
			{
				receipts = receipts.Where(i => i.PerformedBy == filter.UserId);
			}
			var receiptsToShow = receipts
				.OrderBy(r => r.ReceiptNumber)
				.Select(r => new ReceiptSimplyDTO
				{
					ReceiptId = r.Id,
					ReceiptNumber = r.ReceiptNumber,
					ClientId = r.ClientId,
					RampNumber = r.RampNumber,
					PerformedBy = r.PerformedBy,
					ReceiptDateTime = r.ReceiptDateTime,
					ReceiptStatus = r.ReceiptStatus,
				});
			return receiptsToShow.ToPagedResultAsync(pageNumber, pageSize, ct);
		}
	}
}
