using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHistoryPickingRepo
	{
		Task AddHistoryPickingAsync(HistoryPicking historyPicking);
		IQueryable<HistoryPicking> GetAllHistoryPickingAsync();
	}
}
