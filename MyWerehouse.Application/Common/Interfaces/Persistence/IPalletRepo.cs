using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IPalletRepo
	{
		Guid AddPallet(Pallet pallet);
		Task<Pallet?> GetPalletByIdAsync(Guid palletId, CancellationToken ct);
		Task<Pallet?> GetPalletByPalletNumberAsync(string palletNumber, CancellationToken ct);
		Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId, CancellationToken ct);
		Task<List<Pallet>> GetMissingFullPallets(Guid productId,int fullPallet, DateOnly? minBestBefore, int neededPallets, CancellationToken ct);
		Task<List<PalletAllocationCandidate>> GetCandidates(Guid productId, DateOnly? bestBefore, HashSet<Guid> excludedId, CancellationToken ct);
		Task<List<Pallet>> GetSelectedPallets(List<Guid> guids, CancellationToken ct);
		Task<Pallet?> GetPickingPalletByIssueId(Guid issueId, CancellationToken ct);
		Task<int> ReservePalletNumbersAsync(int count, CancellationToken ct);
		Task<Pallet?> CheckOccupancyAsync(int locationId, CancellationToken ct);
		Task<List<Pallet>> GetAvailablePalletsForReversePickingAsync(Guid productId, DateOnly? bestBefore, Guid sourceId, int cartonsPerPallet, CancellationToken ct);
	}
}
