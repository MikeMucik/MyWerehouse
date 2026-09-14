using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IHistoryReversePickingRepo
	{
		void AddHistoryReversePicking(HistoryReversePicking historyReversePicking);
		Task<List<HistoryReversePicking>> GetHistoryReversePickings(CancellationToken ct);
	}
}
