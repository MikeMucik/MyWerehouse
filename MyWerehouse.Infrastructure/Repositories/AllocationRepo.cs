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
		//public async Task AddAllocationApartFromCreatingIssueAsync(int issueId, int productId, int quantity)
		//{
		//	var allocation = new Allocation
		//	{
		//		IssueId = issueId,
		//		Quantity = quantity,

		//	};
		//	await _werehouseDbContext.Allocations.AddAsync(allocation);
		//}

		//public async Task ChangeStatusPalletToPicked(int allocationId)
		//{
		//	var allocation = await _werehouseDbContext.Allocations
		//		.FirstOrDefaultAsync(a => a.Id == allocationId);
		//	//if (allocation == null) { new InvalidDataException($"Brak alokacji o numerze{allocationId}"); }
		//	allocation.PickingStatus = PickingStatus.Picked;
		//	//await _werehouseDbContext.SaveChangesAsync();
		//}

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
		//public async Task<Allocation> GetAllocationAsync(int allocationId)
		//{
		//	return await _werehouseDbContext.Allocations.FirstOrDefaultAsync(a => a.Id == allocationId);
		//}

		
	}
}
