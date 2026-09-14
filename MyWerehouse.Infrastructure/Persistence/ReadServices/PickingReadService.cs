using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree;
using MyWerehouse.Application.Picking.Queries.GetListPickingPalletForOperator;
using MyWerehouse.Application.Picking.Queries.GetListToPickingFlat;
using MyWerehouse.Application.Picking.Queries.PrepareEmergencyPicking;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Infrastructure.Persistence.ReadServices
{
	public class PickingReadService(WerehouseDbContext werehouseDbContext) : IPickingReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		public async Task<List<PickingGuideLineDTO>> GetPickingTaskFlat(DateOnly startDate, DateOnly endDate, CancellationToken ct)
		{
			var lines = await _werehouseDbContext.PickingTasks
				.AsNoTracking()
				.Where(x => x.PickingDay <= endDate &&
				x.PickingDay >= startDate &&
				(x.PickingStatus == PickingStatus.Allocated ||
				x.PickingStatus == PickingStatus.Available))
				.Select(q => new
				{
					q.Issue.ClientId,
					q.IssueId,
					q.Issue.IssueNumber,
					q.ProductId,
					q.Product.SKU,
					q.RequestedQuantity
				})
				.Where(p => p.ProductId != Guid.Empty)
				.GroupBy(p => new
				{
					p.ClientId,
					p.IssueId,
					p.IssueNumber,
					p.ProductId,
					p.SKU,
				})
					.Select(p => new
					{
						p.Key.ClientId,
						p.Key.IssueId,
						p.Key.IssueNumber,
						p.Key.ProductId,
						p.Key.SKU,
						Quantity = p.Sum(q => q.RequestedQuantity)
					})
					.OrderBy(x => x.ClientId)
					.ThenBy(x => x.IssueId)
					.ThenBy(x => x.ProductId)
					.ToListAsync(ct);

			var result = lines
				.GroupBy(x => x.ClientId)
				.Select(clientGroup => new PickingGuideLineDTO
				{
					ClientIdOut = clientGroup.Key,
					IssuesDetailsForPicking = clientGroup
					.GroupBy(x => x.IssueId)
					.Select(issueGroup => new IssueForPickingDTO
					{
						IssueId = issueGroup.Key,
						IssueNumber = issueGroup.First().IssueNumber,
						Products = issueGroup
						.Select(productItem => new ProductOnPalletPickingDTO
						{
							ProductId = productItem.ProductId,
							SKU = productItem.SKU,
							Quantity = productItem.Quantity,
						}).ToList(),
					}).ToList(),
				})
				.OrderBy(x => x.ClientIdOut)
				.ToList();
			return result;
		}

		public async Task<List<ProductToIssueDTO>> GetProductToIssueList(DateOnly startDate, DateOnly endDate, CancellationToken ct)
		{
			var lines = await _werehouseDbContext.PickingTasks
				.AsNoTracking()
				.Where(x => x.PickingDay <= endDate &&
				x.PickingDay >= startDate &&
				(x.PickingStatus == PickingStatus.Allocated ||
				x.PickingStatus == PickingStatus.Available))
				.Select(q => new
				{
					q.Issue.ClientId,
					q.IssueId,
					q.Issue.IssueNumber,
					q.ProductId,
					q.Product.SKU,
					q.RequestedQuantity
				})
				.Where(p => p.ProductId != Guid.Empty)
				.GroupBy(p => new
				{
					p.ClientId,
					p.IssueId,
					p.IssueNumber,
					p.ProductId,
					p.SKU,
				})
					.Select(p => new
					{
						p.Key.ClientId,
						p.Key.IssueId,
						p.Key.IssueNumber,
						p.Key.ProductId,
						p.Key.SKU,
						Quantity = p.Sum(q => q.RequestedQuantity)
					})
					.OrderBy(x => x.ClientId)
					.ThenBy(x => x.IssueId)
					.ThenBy(x => x.ProductId)
					.ToListAsync(ct);

			var result = lines
				.Select(x => new ProductToIssueDTO
				{
					ClientIdOut = x.ClientId,
					IssueId = x.IssueId,
					IssueNumber = x.IssueNumber,
					ProductId = x.ProductId,
					SKU = x.SKU,
					Quantity = x.Quantity,
				}).ToList();
			return result;
		}

		public Task<PagedResult<PickingPalletWithLocationDTO>> GetSourcePalletList(DateOnly startDate, DateOnly endDate, int pageNumber, int pageSize, CancellationToken ct)
		{
			var pickingPallets = _werehouseDbContext.VirtualPallets
				.AsNoTracking()
				.Where(vp =>
				vp.PickingTasks.Any(pt =>
				pt.PickingDay <= endDate && pt.PickingDay >= startDate && pt.PickingStatus == PickingStatus.Allocated))
				.OrderBy(p => p.LocationId)
				.ThenBy(p => p.PalletId);
			var pickingPalletToShow = pickingPallets
				.Select(v => new PickingPalletWithLocationDTO
				{
					PalletId = v.PalletId,
					PalletNumber = v.Pallet.PalletNumber,
					LocationId = v.LocationId,
					AddedToPicking = v.DateMoved,
					LocationName = v.Location.ToSnapshot()
				});
			return pickingPalletToShow.ToPagedResultAsync(pageNumber, pageSize, ct);
		}

		public Task<List<IssueOptions>> GetProperpickingTask(Guid productId, DateOnly start, DateOnly end, CancellationToken ct)
		{
			var result = _werehouseDbContext.PickingTasks
				.AsNoTracking()
				.Where(i => i.Issue.IssueStatus == IssueStatus.New ||
							i.Issue.IssueStatus == IssueStatus.Pending ||
							i.Issue.IssueStatus == IssueStatus.InProgress)
				.Where(a => a.ProductId == productId &&
				(a.PickingStatus == PickingStatus.Allocated || a.PickingStatus == PickingStatus.CorrectionPicking) &&
				a.RequestedQuantity > a.PickedQuantity &&
				a.Issue.IssueDateTimeSend >= start && a.Issue.IssueDateTimeSend <= end);

			var resultToShow = result
				.GroupBy(a => new
				{
					a.IssueId,
					a.Issue.IssueNumber
				})
				.Select(g => new IssueOptions
				{
					IssueId = g.Key.IssueId,
					IssueNumber = g.Key.IssueNumber,
					QuantityToDo = g.Sum(a => a.RequestedQuantity - a.PickedQuantity)
				})
				.ToListAsync(ct);
			return resultToShow;
		}

		public async Task<PagedResult<PickingTaskDTO>> GetPickingTaskForPallet(Guid palletId, DateOnly pickingDate, int pageNumber, int pageSize, CancellationToken ct)
		{

			var result = _werehouseDbContext.PickingTasks
				.AsNoTracking()
				.Where(p =>
					p.VirtualPallet!.PalletId == palletId &&
					DateOnly.FromDateTime(p.Issue.IssueDateTimeCreate) >= pickingDate.AddDays(-14) &&//ustalenie biznesowe
					p.Issue.IssueDateTimeSend >= pickingDate &&
					p.Issue.IssueDateTimeSend < pickingDate.AddDays(2) &&
					p.PickingStatus == PickingStatus.Allocated);
			var resultToShow = result
				.Select(p => new PickingTaskDTO
				{
					Id = p.Id,
					IssueId = p.IssueId,
					IssueNumber = p.Issue.IssueNumber,
					SourcePalletId = p.VirtualPallet!.PalletId,
					SourcePalletNumber = p.VirtualPallet.Pallet.PalletNumber,
					ProductId = p.ProductId,
					SKU = p.Product.SKU,
					RequestedQuantity = p.RequestedQuantity,
					PickingStatus = p.PickingStatus,
					BestBefore = p.BestBefore
				})
				.OrderBy(p => p.IssueNumber)
				.ThenBy(p => p.SKU)
				.ThenBy(p => p.Id);
			return await resultToShow.ToPagedResultAsync(pageNumber, pageSize, ct);
		}
	}
}
