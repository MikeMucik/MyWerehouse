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
	public class PickingPalletRepo : IPickingPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PickingPalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task<int> AddPalletToPickingAsync(string palletId)
		{
			var pallet = await _werehouseDbContext.Pallets
				.FirstAsync(p => p.Id == palletId);

			var newVirtualPicking = new VirtualPallet
			{
				Pallet = pallet,
				PalletId = palletId,
				DateMoved = DateTime.UtcNow,
				LocationId = pallet.LocationId,
				IssueInitialQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == palletId).Quantity,//zakładam że jest jeden towar
				Allocation = new List<Allocation>()
			};
			await _werehouseDbContext.VirtualPallets.AddAsync(newVirtualPicking);
			return newVirtualPicking.Id;
		}
		public async Task DeleteVirtualPickingAsync(int id)
		{
			var pallet = await _werehouseDbContext.VirtualPallets.FindAsync(id);
			if (pallet == null) { throw new InvalidOperationException("Brak palety do usunięcia"); }
			_werehouseDbContext.VirtualPallets.Remove(pallet);
		}
		public async Task<Allocation> AddAllocationAsync(VirtualPallet pallet, int issueId, int quantity)
		{
			var allocation = new Allocation
			{
				IssueId = issueId,
				VirtualPallet = pallet,
				VirtualPalletId = pallet.Id,
				Quantity = quantity,
				PickingStatus = PickingStatus.Allocated,
			};
			await _werehouseDbContext.Allocations.AddAsync(allocation);
			//await _werehouseDbContext.SaveChangesAsync();
			var pickingPallet = await _werehouseDbContext.VirtualPallets.FindAsync(pallet.Id);
			if (pickingPallet == null) { throw new InvalidOperationException("Brak palety do pickingu"); }
			if (pickingPallet.RemainingQuantity <= 0)
			{
				await DeleteVirtualPickingAsync(pallet.Id);
			}
			return allocation;
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsAsync(int productId)
		{
			var list = await _werehouseDbContext.VirtualPallets
					.Include(a => a.Allocation)
					.Include(p => p.Pallet)
						.ThenInclude(pp => pp.ProductsOnPallet)
					.Where(p => p.Pallet.ProductsOnPallet.Any(p => p.ProductId == productId) && p.Pallet.Status == PalletStatus.ToPicking)
					.OrderBy(p => p.IssueInitialQuantity - p.Allocation.Sum(a => a.Quantity))
					.ToListAsync();
			return list;
		}

		public async Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end)
		{
			var list = await _werehouseDbContext.VirtualPallets
				.Include(a => a.Allocation)
				.Include(p => p.Pallet)
					.ThenInclude(pp => pp.ProductsOnPallet)
				.Where(p => p.DateMoved > start && p.DateMoved < end)
				.ToListAsync();
			return list;
		}

		public async Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId)
		{
			var pickingPallet = await _werehouseDbContext.VirtualPallets.FirstOrDefaultAsync(p => p.Id == pickingPalletId);
			if (pickingPallet == null) { throw new InvalidDataException($"Brak palety pickingowej o numerze {pickingPalletId}"); }
			return pickingPallet.DateMoved;
		}

		public async Task<int> GetVirtualPalletIdFromPalletIdAsync(string palletId)
		{
			var palletPicking = await _werehouseDbContext.VirtualPallets
				.FirstOrDefaultAsync(p => p.PalletId == palletId);
			return palletPicking.Id;
		}
		public async Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate)
		{
			var allocation = await _werehouseDbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(b => b.Pallet)
						.ThenInclude(c => c.ProductsOnPallet)
				.Include(i=>i.Issue)
				.Where(p =>
					p.VirtualPalletId == palletPickingId &&
					p.Issue.IssueDateTimeCreate > pickingDate.AddDays(-7) &&
					(
						p.Issue.IssueDateTimeSend == pickingDate.Date ||
						p.Issue.IssueDateTimeSend == pickingDate.AddDays(-1).Date
					) &&
					p.PickingStatus == PickingStatus.Allocated)
				.ToListAsync();
			return allocation;
		}
		public async Task<Allocation> GetAllocationAsync(int allocationId)
		{
			return await _werehouseDbContext.Allocations.FirstOrDefaultAsync(a => a.Id == allocationId);
		}

		public async Task<VirtualPallet> GetVirtualPalletByIdAsync(int palletId)
		{
			return await _werehouseDbContext.VirtualPallets.FirstAsync(p => p.Id == palletId);
		}

		public async Task<List<Allocation>> GetAllocationsByIssueIdProductIdAsync(int issueId, int productId)
		{
			var result = await _werehouseDbContext.Allocations
				.Include(i => i.Issue)
				.Where(a=>a.IssueId == issueId && a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == productId)
				.ToListAsync();
			return result;
		}

		public async Task<List<Allocation>> GetAllocationByEntityAsync(Issue issue, Product product)
		{
			return await _werehouseDbContext.Allocations
	   .Include(a => a.Issue)
	   .Include(a => a.VirtualPallet)
		   .ThenInclude(pp => pp.Pallet)
	   .Where(a => a.Issue.Id == issue.Id &&
								 a.VirtualPallet.Pallet.ProductsOnPallet.Any(p => p.Product.Id == product.Id)).ToListAsync();
		}

		//public async Task<List<PickingPallet>> GetPickingPalletsByProductAsync(int productId)
		//{
		//	return await _werehouseDbContext.PickingPallets
		//		.Include(a=>a.Allocation)
		//		.Include(p=>p.Pallet)
		//			.ThenInclude(pp=>pp.ProductsOnPallet)
		//		.Where(a=>a.)
		//}
	}
}
