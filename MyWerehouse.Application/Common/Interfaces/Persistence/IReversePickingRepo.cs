using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IReversePickingRepo
	{
		void AddReversePicking(ReversePickingTask reversePicking);
		Task<ReversePickingTask?> GetReversePickingAsync(Guid reversePickingId, CancellationToken ct);
		IQueryable<ReversePickingTask> GetReversePickings();
		Task<bool> ExistsForPickingPalletAsync(Guid palletId, CancellationToken ct);
		Task<List<Guid>> GetPalletsIdsByDate(DateOnly start, DateOnly end, CancellationToken ct);
	}
}
