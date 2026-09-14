using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IVirtualPalletRepo
	{
		VirtualPallet AddPalletToPicking(VirtualPallet virtualPallet);
		void DeleteVirtualPalletPicking(VirtualPallet virtualPallet);
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid productId, CancellationToken ct);
		Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end, CancellationToken ct);
		Task<List<VirtualPallet>> GetVirtualPalletsByBBAsync(Guid productId, DateOnly? bestBefore, CancellationToken ct);
		Task<VirtualPallet?> GetVirtualPalletByIdAsync(Guid? palletId, CancellationToken ct);
		Task<VirtualPallet?> GetVirtualPalletByPalletIdAsync(Guid palletId, CancellationToken ct);//add test repo
	}
}
