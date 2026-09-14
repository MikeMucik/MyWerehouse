using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IHistoryPalletRepo
	{
		void AddHistoryPallet(HistoryPallet palletMovement);
		Task<bool> CanDeletePalletAsync(Guid palletId, CancellationToken ct);
	}
}
