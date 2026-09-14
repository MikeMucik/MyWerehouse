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
	public class PickingTaskRepo(WerehouseDbContext werehouseDbContext) : IPickingTaskRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public void AddPickingTask(PickingTask pickingTask)
		{
			_werehouseDbContext.PickingTasks.Add(pickingTask);
		}
		public async Task AddPickingTaskAsync(PickingTask pickingTask, CancellationToken ct)
		{
			await _werehouseDbContext.PickingTasks.AddAsync(pickingTask, ct);
		}
		public void DeletePickingTask(PickingTask pickingTask)
		{
			_werehouseDbContext.PickingTasks.Remove(pickingTask);
		}		
		public async Task<PickingTask?> GetPickingTaskAsync(Guid guid, CancellationToken ct)
		{
			return await _werehouseDbContext.PickingTasks
				.Include(v => v.VirtualPallet)
				.SingleOrDefaultAsync(a => a.Id == guid, ct);
		}
		public async Task<List<PickingTask>> GetPickingTasksByIssueIdProductIdAsync(Guid issueId, Guid productId, CancellationToken ct)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId && a.ProductId == productId)
				.ToListAsync(ct);
			return result;
		}
		public async Task<List<PickingTask>> GetPickingTasksByIssueIdAsync(Guid issueId, CancellationToken ct)
		{
			var result = await _werehouseDbContext.PickingTasks
				.Include(i => i.Issue)
				.Include(p => p.Product)
				.Where(a => a.IssueId == issueId)
				.Where(t => t.PickingStatus == PickingStatus.Allocated ||
				t.PickingStatus == PickingStatus.CorrectionPicking)
				.ToListAsync(ct);
			return result;
		}
		public async Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(Guid pickingPalletId, CancellationToken ct)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(x => x.PickingPalletId == pickingPalletId)
				.Where(x => x.VirtualPallet != null)
				.Include(x => x.VirtualPallet!)
					.ThenInclude(xp => xp.Pallet)
						.ThenInclude(p => p.ProductsOnPallet)
				.ToListAsync(ct);
		}
		public async Task<List<PickingTask>> GetHandPickingTask(Guid issueId, CancellationToken ct)
		{
			return await _werehouseDbContext.PickingTasks
				.Where(p => p.IssueId == issueId && p.VirtualPallet == null && p.PickingStatus == PickingStatus.Available)
				.ToListAsync(ct);
		}
	}
}
