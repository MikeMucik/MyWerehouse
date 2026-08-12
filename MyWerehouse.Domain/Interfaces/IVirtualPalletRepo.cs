using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IVirtualPalletRepo
	{
		VirtualPallet AddPalletToPicking(VirtualPallet virtualPallet);
		void DeleteVirtualPalletPicking(VirtualPallet virtualPallet);
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid productId, CancellationToken ct);
		Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end, CancellationToken ct);
		IQueryable<VirtualPallet> GetVirtualPalletsByTimePickingTask(DateOnly start, DateOnly end);
		Task<List<VirtualPallet>> GetVirtualPalletsByBBAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct);
		Task<Guid> GetVirtualPalletIdFromPalletIdAsync(Guid palletId, CancellationToken ct);
		Task<VirtualPallet?> GetVirtualPalletByIdAsync(Guid? palletId, CancellationToken ct);
		Task<VirtualPallet?> GetVirtualPalletByPalletIdAsync(Guid palletId, CancellationToken ct);//add test repo
	}
}
