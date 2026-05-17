using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class PickingTaskRepo : IPickingTaskRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PickingTaskRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddPickingTask(PickingTask pickingTask)
		{
			_werehouseDbContext.PickingTasks.Add(pickingTask);
		}
		public async Task AddPickingTaskAsync(PickingTask pickingTask)
		{
			await _werehouseDbContext.PickingTasks.AddAsync(pickingTask);
		}
		public void DeletePickingTask(PickingTask pickingTask)
		{
			_werehouseDbContext.PickingTasks.Remove(pickingTask);
		}
		public IQueryable<PickingTask> GetPickingTaskList(Guid palletPickingId, DateTime pickingDate)
		{
			var pickingTask = _werehouseDbContext.PickingTasks
				
				.Include(a => a.VirtualPallet)
					.ThenInclude(b => b.Pallet)
						.ThenInclude(c => c.ProductsOnPallet)
				.Include(i => i.Issue)
				.Where(p =>
					p.VirtualPalletId == palletPickingId &&
					p.Issue.IssueDateTimeCreate >= pickingDate.AddDays(-14) &&//ustalenie biznesowe
					p.Issue.IssueDateTimeSend >= pickingDate &&
					p.Issue.IssueDateTimeSend < pickingDate.AddDays(2) &&
					p.PickingStatus == PickingStatus.Allocated);
			return pickingTask;
		}
		//.Include(a => a.VirtualPallet)//
		//			.ThenInclude(p => p.Pallet)//
		//				.ThenInclude(l => l.Location)//


		public async Task<PickingTask?> GetPickingTaskAsync(Guid guid)
		{
			return await _werehouseDbContext.PickingTasks.SingleOrDefaultAsync(a => a.Id == guid);
		}
		public async Task<List<PickingTask>> GetPickingTasksByIssueIdProductIdAsync(Guid issueId, Guid productId)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId && a.ProductId == productId)
				.ToListAsync();
			return result;
		}
		public async Task<List<PickingTask>> GetPickingTasksProductIdAsync(Guid productId, DateTime from, DateTime to)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.ProductId == productId &&
				a.PickingStatus == PickingStatus.Allocated &&
				a.RequestedQuantity > 0 &&
				a.Issue.IssueDateTimeSend > from && a.Issue.IssueDateTimeSend < to)
				.ToListAsync();
			return result;
		}
		public async Task<List<PickingTask>> GetPickingTasksByIssueIdAsync(Guid issueId)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId)
				.ToListAsync();
			return result;
		}

		public async Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(Guid pickingPalletId)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x => x.PickingPalletId == pickingPalletId)
				.ToListAsync();
		}

		public IQueryable<PickingTaskFlat> GetPickingTaskFlats(DateOnly start, DateOnly end)
		{
			var list = _werehouseDbContext.PickingTasks
				.AsNoTracking()
				.Where(x => x.PickingDay <= end &&
				x.PickingDay >= start &&
				(x.PickingStatus == PickingStatus.Allocated ||
				x.PickingStatus == PickingStatus.Available))
				.Select(q => new
				{
					q.Issue.ClientId,
					q.IssueId,
					q.Issue.IssueNumber,
					q.ProductId,
					q.RequestedQuantity
				})
				.Where(p => p.ProductId != Guid.Empty)
				.GroupBy(p => new
				{
					p.ClientId,
					p.IssueId,
					p.IssueNumber,
					p.ProductId,
				})
				.Select(p => new PickingTaskFlat
				{
					ClientId = p.Key.ClientId,
					IssueId = p.Key.IssueId,
					IssueNumber = p.Key.IssueNumber,
					ProductId = p.Key.ProductId,
					Quantity = p.Sum(q => q.RequestedQuantity)
				});
			return list;
		}
	}
}