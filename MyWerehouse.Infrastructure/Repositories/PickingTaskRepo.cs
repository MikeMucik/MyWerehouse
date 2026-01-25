using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Repositories
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
					p.Issue.IssueDateTimeCreate > pickingDate.AddDays(-7) &&
					(
						p.Issue.IssueDateTimeSend == pickingDate.Date ||
						p.Issue.IssueDateTimeSend == pickingDate.AddDays(1).Date
					) &&
					p.PickingStatus == PickingStatus.Allocated)
				.ToListAsync();
			return pickingTask;
		}
		public async Task<PickingTask> GetPickingTaskAsync(int pickingTaskId)
		{
			return await _werehouseDbContext.PickingTasks.FirstOrDefaultAsync(a => a.Id == pickingTaskId);
		}
		public async Task<List<PickingTask>> GetPickingTasksByIssueIdProductIdAsync(int issueId, int productId)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId && a.ProductId == productId)
				.ToListAsync();
			return result;
		}
		public async Task<List<PickingTask>> GetPickingTasksProductIdAsync(int productId, DateTime from, DateTime to)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.ProductId == productId &&
				a.PickingStatus == PickingStatus.Allocated &&
				a.Quantity > 0 &&
				(a.Issue.IssueDateTimeSend > from && a.Issue.IssueDateTimeSend < to))
				.ToListAsync();
			return result;
		}
		public async Task<List<PickingTask>> GetPickingTasksByIssueIdAsync(int issueId)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId)
				.ToListAsync();
			return result;
		}

		public async Task<List<VirtualPallet>> GetVirtualPalletsByIssue(int issueId)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x=>x.IssueId == issueId)
				.Select(x=>x.VirtualPallet)
				.Distinct()
				.ToListAsync();
		}

		public async Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(string pickingPalletId)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x=>x.PickingPalletId == pickingPalletId)
				.ToListAsync();
		}


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
