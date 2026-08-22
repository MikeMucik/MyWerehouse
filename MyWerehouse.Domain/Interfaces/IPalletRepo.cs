using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPalletRepo
	{
		Guid AddPallet(Pallet pallet);
		Task<Pallet?> GetPalletByIdAsync(Guid palletId, CancellationToken ct);
		Task<Pallet?> GetPalletByIdFullInfoAsync(Guid palletId, CancellationToken ct);
		Task<Pallet?> GetPalletByPalletNumberAsync(string palletNumber, CancellationToken ct);
		Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId, CancellationToken ct);
		Task<List<Pallet>> GetMissingFullPallets(Guid productId,int fullPallet, DateOnly? minBestBefore, int neededPallets, CancellationToken ct);
		Task<List<PalletAllocationCandidate>> GetCandidates(Guid productId, DateOnly? bestBefore, HashSet<Guid> excludedId, CancellationToken ct);
		Task<List<Pallet>> GetSelectedPallets(List<Guid> guids, CancellationToken ct);
		Task<Pallet?> GetPickingPalletByIssueId(Guid issueId, CancellationToken ct);
		IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter);
		Task<int> ReservePalletNumbersAsync(int count, CancellationToken ct);
		Task<Pallet?> CheckOccupancyAsync(int locationId, CancellationToken ct);
		Task<List<Pallet>> GetAvailablePalletsForReversePickingAsync(Guid productId, DateOnly? bestBefore, Guid sourceId, int cartonsPerPallet, CancellationToken ct);
	}
}
