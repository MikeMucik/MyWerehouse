using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IHistoryPickingRepo
	{
		void AddHistoryPicking(HistoryPicking historyPicking);
		IQueryable<HistoryPicking> GetAllHistoryPickingAsync();
	}
}
