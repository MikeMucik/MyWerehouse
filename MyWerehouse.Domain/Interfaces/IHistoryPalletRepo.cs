using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHistoryPalletRepo
	{		
		void AddHistoryPallet(HistoryPallet palletMovement);
		Task< List<HistoryPallet>> GetHistoryPallet(string PalletNumber);
		Task<bool> CanDeletePalletAsync(Guid palletId);
	}
}
