using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHistoryReversePickingRepo
	{
		void AddHistoryReversePicking(HistoryReversePicking historyReversePicking);
		Task<List<HistoryReversePicking>> GetHistoryReversePickings();
	}
}
