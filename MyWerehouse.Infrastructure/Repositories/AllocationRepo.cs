using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class AllocationRepo : IAllocationRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public AllocationRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}		
		public void AddAllocation(Allocation allocation)
		{			
			_werehouseDbContext.Allocations.Add(allocation);    			
		}
		public void DeleteAllocation(Allocation allocation)
		{			
			_werehouseDbContext.Allocations.Remove(allocation);
		}
		public async Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate)
		{
			var allocation = await _werehouseDbContext.Allocations
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
			return allocation;
		}
		public async Task<Allocation> GetAllocationAsync(int allocationId)
		{
			return await _werehouseDbContext.Allocations.FirstOrDefaultAsync(a => a.Id == allocationId);
		}
		public async Task<List<Allocation>> GetAllocationsByIssueIdProductIdAsync(int issueId, int productId)
		{
			var result = await _werehouseDbContext.Allocations
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId && a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == productId)
				.ToListAsync();
			return result;
		}
		public async Task<List<Allocation>> GetAllocationsProductIdAsync(int productId, DateTime from, DateTime to)
		{
			var result = await _werehouseDbContext.Allocations
				.Include(i => i.Issue)
				.Where(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == productId &&
				a.PickingStatus == PickingStatus.Allocated &&
				a.Quantity > 0 &&
				(a.Issue.IssueDateTimeSend > from && a.Issue.IssueDateTimeSend < to))
				.ToListAsync();
			return result;
		}
		public async Task<List<Allocation>> GetAllocationsByIssueIdAsync(int issueId)
		{
			var result = await _werehouseDbContext.Allocations
				.Include(i => i.Issue)
				.Where(a => a.IssueId == issueId)
				.ToListAsync();
			return result;
		}

		public async Task<List<VirtualPallet>> GetVirtualPalletsByIssue(int issueId)
		{
			return await _werehouseDbContext.Allocations
				.Where(x=>x.IssueId == issueId)
				.Select(x=>x.VirtualPallet)
				.Distinct()
				.ToListAsync();
		}


		//public async Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate)
		//{
		//	var allocation = await _werehouseDbContext.Allocations
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
		//	return allocation;
		//}


	}
}
