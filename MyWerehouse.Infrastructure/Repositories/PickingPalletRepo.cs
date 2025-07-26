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
				PalletId = palletId,
				DateMoved = DateTime.UtcNow,
				IssueInitialQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == palletId).Quantity,//zakładam że jest jeden towar
				Allocation = new List<Allocation>()
			};
			await _werehouseDbContext.PickingPallets.AddAsync(newPalletPicking);
		}
		public async Task DeletePalletPickingAsync(int id)
		{
			var pallet = await _werehouseDbContext.PickingPallets.FindAsync(id);
			_werehouseDbContext.PickingPallets.Remove(pallet);
		}
		public async Task AddAllocationAsync(int id, int issueId, int quantity)
		{
			await _werehouseDbContext.Allocations.AddAsync(new Allocation
			{
				IssueId = issueId,
				PickingPalletId = id,
				Quantity = quantity,
				PickingStatus = PickingStatus.Allocated,
			});
			var pickingPallet = await _werehouseDbContext.PickingPallets.FindAsync(id);

			if (pickingPallet.RemainingQuantity <= 0)
			{
				await DeletePalletPickingAsync(id);
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
		//public async Task UpdatePalletPickingAsync(string palletId, int issueId, int quantity)
		//{
		//	var pickingPallet = await _werehouseDbContext.PickingPallets.FirstOrDefaultAsync(p => p.PalletId == palletId);
		//	if (pickingPallet == null) { throw new InvalidOperationException("Brak palety"); }
		//	//var newPalletPicking = new PickingPallets
		//	//{
		//	//	PalletId = palletId,
		//	//	DateMoved = DateTime.UtcNow,
		//	//	IssueInitialQuantity = pickingPallet.IssueInitialQuantity
		//	//};
		//	await _werehouseDbContext.Allocations.AddAsync(new Allocation { PickingPalletId = palletId, IssueId = issueId, Quantity = quantity });
		//	if (pickingPallet.RemainingQuantity <= 0)
		//	{
		//		await DeletePalletPickingAsync(palletId);
		//	}
		//	//await _werehouseDbContext.PickingPallets.AddAsync(newPalletPicking);
		//	//await _werehouseDbContext.SaveChangesAsync();
		//}
	}
}
