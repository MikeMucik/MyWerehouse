using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHistoryReceiptRepo
	{
		void AddHistoryReceipt(HistoryReceipt historyReceipt);
		Task AddHistoryReceiptAsync(HistoryReceipt historyReceipt, CancellationToken cancellationToken);
		IQueryable<HistoryReceipt> GetAllHistoryReceipt();
	}
}
