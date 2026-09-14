using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IHistoryReceiptRepo
	{
		void AddHistoryReceipt(HistoryReceipt historyReceipt);
		IQueryable<HistoryReceipt> GetAllHistoryReceipt();
	}
}
