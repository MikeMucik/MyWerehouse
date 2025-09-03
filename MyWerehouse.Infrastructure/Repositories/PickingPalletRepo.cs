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

		public async Task AddPalletToPickingAsync(string palletId)
		{
			var pallet = await _werehouseDbContext.Pallets
				.FirstAsync(p => p.Id == palletId);

			var newPalletPicking = new PickingPallet
			{
				Pallet = pallet,
				PalletId = palletId,
				DateMoved = DateTime.UtcNow,
				LocationId = pallet.LocationId,
				IssueInitialQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == palletId).Quantity,//zakładam że jest jeden towar
				Allocation = new List<Allocation>()
			};
			await _werehouseDbContext.PickingPallets.AddAsync(newPalletPicking);
		}
		public async Task DeletePalletPickingAsync(int id)
		{
			var pallet = await _werehouseDbContext.PickingPallets.FindAsync(id);
			if (pallet != null) { throw new InvalidOperationException("Brak palety do usunięcia"); }
			_werehouseDbContext.PickingPallets.Remove(pallet);
		}
		public async Task AddAllocationAsync(PickingPallet pallet, int issueId, int quantity)
		{
			await _werehouseDbContext.Allocations.AddAsync(new Allocation
			{
				IssueId = issueId,
				PickingPallet = pallet,
				PickingPalletId = pallet.Id,
				Quantity = quantity,
				PickingStatus = PickingStatus.Allocated,
			});
			var pickingPallet = await _werehouseDbContext.PickingPallets.FindAsync(pallet.Id);
			if (pickingPallet == null) { throw new InvalidOperationException("Brak palety do pickingu"); }
			if (pickingPallet.RemainingQuantity <= 0)
			{
				await DeletePalletPickingAsync(pallet.Id);
			}
		}
		public async Task<List<PickingPallet>> GetPickingPalletsAsync(int productId)
		{
			var list = await _werehouseDbContext.PickingPallets
					.Include(a => a.Allocation)
					.Include(p => p.Pallet)
						.ThenInclude(pp => pp.ProductsOnPallet)
					.Where(p => p.Pallet.ProductsOnPallet.Any(p => p.ProductId == productId) && p.Pallet.Status == PalletStatus.ToPicking)
					.OrderBy(p => p.IssueInitialQuantity - p.Allocation.Sum(a => a.Quantity))
					.ToListAsync();
			return list;
		}

		public async Task<List<PickingPallet>> GetPickingPalletsByTimeAsync(DateTime start, DateTime end)
		{
			var list = await _werehouseDbContext.PickingPallets
				.Include(a => a.Allocation)
				.Include(p => p.Pallet)
					.ThenInclude(pp => pp.ProductsOnPallet)
				.Where(p => p.DateMoved > start && p.DateMoved < end)
				.ToListAsync();
			return list;
		}

		public async Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId)
		{
			var pickingPallet = await _werehouseDbContext.PickingPallets.FirstOrDefaultAsync(p => p.Id == pickingPalletId);
			if (pickingPallet == null) { throw new InvalidDataException($"Brak palety pickingowej o numerze {pickingPalletId}"); }
			return pickingPallet.DateMoved;
		}
		
		public async Task<int> GetPickingPalletIdFromPalletIdAsync(string palletId)
		{
			var palletPicking = await _werehouseDbContext.PickingPallets
				.FirstOrDefaultAsync(p=>p.PalletId == palletId);
			return palletPicking.Id;
		}
		public async Task<List<Allocation>> GetAllocationListAsync(int palletPickingId, DateTime pickingDate)
		{
			var allocation = await _werehouseDbContext.Allocations
				.Include(a => a.PickingPallet)
					.ThenInclude(b => b.Pallet)
						.ThenInclude(c => c.ProductsOnPallet)
				.Where(p =>
					p.PickingPalletId == palletPickingId &&
					p.Issue.IssueDateTimeCreate > pickingDate.AddDays(-7) &&
					p.Issue.IssueDateTimeSend != null &&
					(
						p.Issue.IssueDateTimeSend.Value.Date == pickingDate.Date ||
						p.Issue.IssueDateTimeSend.Value.Date == pickingDate.AddDays(-1).Date
					) &&
					p.PickingStatus == PickingStatus.Allocated)
				.ToListAsync();
			return allocation;
		}
		public async Task<Allocation> GetAllocationAsync(int allocationId)
		{
			return await _werehouseDbContext.Allocations.FirstOrDefaultAsync(a => a.Id == allocationId);
		}

		public async Task<PickingPallet> GetPickingPalletByIdAsync(int palletId)
		{
			return await _werehouseDbContext.PickingPallets.FirstAsync(p => p.Id == palletId);
		}
	}
}
