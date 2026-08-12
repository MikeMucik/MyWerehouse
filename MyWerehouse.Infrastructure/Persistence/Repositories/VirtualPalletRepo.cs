using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{

	public class VirtualPalletRepo : IVirtualPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public VirtualPalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public VirtualPallet AddPalletToPicking(VirtualPallet newVirtualPicking)
		{
			_werehouseDbContext.VirtualPallets.Add(newVirtualPicking);
			return newVirtualPicking;
		}
		public void DeleteVirtualPalletPicking(VirtualPallet virtualPallet)
		{
			_werehouseDbContext.VirtualPallets.Remove(virtualPallet);
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid productId, CancellationToken ct)
		{
			var list = await _werehouseDbContext.VirtualPallets
					.Include(a => a.PickingTasks)
					.Include(p => p.Pallet)
						.ThenInclude(pp => pp.ProductsOnPallet)
					.Where(p => p.Pallet.ProductsOnPallet.Any(p => p.ProductId == productId) && p.Pallet.Status == PalletStatus.ToPicking)
					.OrderBy(p => p.InitialPalletQuantity - p.PickingTasks.Sum(a => a.RequestedQuantity))
					.ToListAsync(ct);
			return list;
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end, CancellationToken ct)
		{
			var list = await _werehouseDbContext.VirtualPallets
				.Include(a => a.PickingTasks)
				.Include(p => p.Pallet)
					.ThenInclude(pp => pp.ProductsOnPallet)
				.Where(p => p.DateMoved >= start && p.DateMoved <= end)
				.ToListAsync(ct);
			return list;
		}

		public IQueryable<VirtualPallet> GetVirtualPalletsByTimePickingTask(DateOnly start, DateOnly end)
		{
			var list = _werehouseDbContext.VirtualPallets
				.Include(a => a.PickingTasks)
				.Include(p => p.Pallet)
					.ThenInclude(l=>l.Location)
				.Where(vp =>
				vp.PickingTasks.Any(pt =>
				pt.PickingDay <= end && pt.PickingDay >= start && pt.PickingStatus == PickingStatus.Allocated));
			return list;
		}
		public async Task<Guid> GetVirtualPalletIdFromPalletIdAsync(Guid palletId, CancellationToken ct)
		{
			var palletPicking = await _werehouseDbContext.VirtualPallets
				.FirstOrDefaultAsync(p => p.PalletId == palletId, ct);
			if (palletPicking == null) { return Guid.Empty; }
			return palletPicking.Id;
		}
		public async Task<VirtualPallet?> GetVirtualPalletByIdAsync(Guid? palletId, CancellationToken ct)
		{
			return await _werehouseDbContext.VirtualPallets
				.Include(p=>p.PickingTasks)
				.FirstAsync(p => p.Id == palletId, ct);
		}

		public async Task<List<VirtualPallet>> GetVirtualPalletsByBBAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct)
		{
			return await _werehouseDbContext.VirtualPallets
				.Where(v => v.Pallet.ProductsOnPallet.First().ProductId == productId
				&&(bestBefore == null|| v.Pallet.ProductsOnPallet.First().BestBefore >= bestBefore))
				.Include(p => p.PickingTasks)
				.Include(p=>p.Pallet)
					.ThenInclude(l=>l.Location)
				.ToListAsync(ct);
		}

		public async Task<VirtualPallet?> GetVirtualPalletByPalletIdAsync(Guid palletId, CancellationToken ct)
		{
			return await _werehouseDbContext.VirtualPallets
				.Include(p=>p.PickingTasks)
				.FirstOrDefaultAsync(v=>v.PalletId == palletId, ct);
		}
	}
}
