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
		public async Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid productId)
		{
			var list = await _werehouseDbContext.VirtualPallets
					.Include(a => a.PickingTasks)
					.Include(p => p.Pallet)
						.ThenInclude(pp => pp.ProductsOnPallet)
					.Where(p => p.Pallet.ProductsOnPallet.Any(p => p.ProductId == productId) && p.Pallet.Status == PalletStatus.ToPicking)
					.OrderBy(p => p.InitialPalletQuantity - p.PickingTasks.Sum(a => a.RequestedQuantity))
					.ToListAsync();
			return list;
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end)
		{
			var list = await _werehouseDbContext.VirtualPallets
				.Include(a => a.PickingTasks)
				.Include(p => p.Pallet)
					.ThenInclude(pp => pp.ProductsOnPallet)
				.Where(p => p.DateMoved >= start && p.DateMoved <= end)
				.ToListAsync();
			return list;
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsByTimePickingTaskAsync(DateOnly start, DateOnly end)
		{
			var list = await _werehouseDbContext.VirtualPallets
				.Include(a => a.PickingTasks)
				.Include(p => p.Pallet)
					.ThenInclude(pp => pp.ProductsOnPallet)
				.Where(vp=> 
				vp.PickingTasks.Any(pt=> 
				pt.PickingDay <= end && pt.PickingDay >= start && pt.PickingStatus == PickingStatus.Allocated))
				.ToListAsync();
			return list;
		}
		public async Task<Guid> GetVirtualPalletIdFromPalletIdAsync(Guid palletId)
		{
			var palletPicking = await _werehouseDbContext.VirtualPallets
				.FirstOrDefaultAsync(p => p.PalletId == palletId);
			if (palletPicking == null) { return Guid.Empty; }
			return palletPicking.Id;
		}
		public async Task<VirtualPallet?> GetVirtualPalletByIdAsync(Guid? palletId)
		{
			return await _werehouseDbContext.VirtualPallets.FirstAsync(p => p.Id == palletId);
		}
		
		public async Task<List<VirtualPallet>> GetVirtualPalletsByBBAsync(Guid productId, DateOnly bestBefore)
		{
			return await _werehouseDbContext.VirtualPallets.Where(v => v.Pallet.ProductsOnPallet.First().ProductId == productId).ToListAsync();
		}

		public async Task<VirtualPallet?> GetVirtualPalletByPalletIdAsync(Guid palletId)
		{
			return await _werehouseDbContext.VirtualPallets.FirstOrDefaultAsync(v=>v.PalletId == palletId);
		}

		
	}
}
