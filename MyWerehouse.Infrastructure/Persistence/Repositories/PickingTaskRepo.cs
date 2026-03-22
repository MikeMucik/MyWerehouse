using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public async Task<List<PickingTask>> GetPickingTaskListAsync(int palletPickingId, DateTime pickingDate)
		{
			var pickingTask = await _werehouseDbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(b => b.Pallet)
						.ThenInclude(c => c.ProductsOnPallet)
				.Include(i => i.Issue)
				.Where(p =>
					p.VirtualPalletId == palletPickingId &&
					p.Issue.IssueDateTimeCreate >= pickingDate.AddDays(-7) &&
					p.Issue.IssueDateTimeSend >= pickingDate &&
					p.Issue.IssueDateTimeSend < pickingDate.AddDays(2) &&
					p.PickingStatus == PickingStatus.Allocated)
				.ToListAsync();
			return pickingTask;
		}
		//public async Task<PickingTask?> GetPickingTaskAsync(int pickingTaskNumber)
		//{
		//	return await _werehouseDbContext.PickingTasks.SingleOrDefaultAsync(a => a.PickingTaskNumber == pickingTaskNumber);
		//}
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

		public async Task<List<VirtualPallet>> GetVirtualPalletsByIssue(Guid issueId)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x => x.IssueId == issueId)
				.Select(x => x.VirtualPallet)
				.Distinct()
				.ToListAsync();
		}

		public async Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(Guid pickingPalletId)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x => x.PickingPalletId == pickingPalletId)
				.ToListAsync();
		}

		//public async Task<int> GetNextNumberOfPickingTask()
		//{
		//	var number = await _werehouseDbContext.PickingTasks.MaxAsync(t => (int?)t.PickingTaskNumber) ?? 0;
		//	return number + 1;
		//}


		//public async Task<List<PickingTask>> GetPickingTaskListAsync(int palletPickingId, DateTime pickingDate)
		//{
		//	var pickingTask = await _werehouseDbContext.PickingTasks
		//		.Include(a => a.PickingPallet)
		//			.ThenInclude(b => b.Pallet)
		//				.ThenInclude(c => c.ProductsOnPallet)
		//		.Where(p =>
		//			p.PickingPalletId == palletPickingId &&
		//			p.Issue.IssueDateTimeCreate > pickingDate.AddDays(-7) &&
		//			p.Issue.IssueDateTimeSend != null &&
		//			(
		//				p.Issue.IssueDateTimeSend.Value.Date == pickingDate.Date ||
		//				p.Issue.IssueDateTimeSend.Value.Date == pickingDate.AddDays(-1).Date
		//			) &&
		//			p.PickingStatus == PickingStatus.Allocated)
		//		.ToListAsync();
		//	return pickingTask;
		//}


	}
}
