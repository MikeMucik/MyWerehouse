using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Filters;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHistoryPalletRepo
	{		
		void AddHistoryPallet(HistoryPallet palletMovement);
		IQueryable<HistoryPallet> GetDataByFilter(HistoryPalletSearchFilter filter, Guid PalletId);		
		Task<bool> CanDeletePalletAsync(Guid palletId);
	}
}
