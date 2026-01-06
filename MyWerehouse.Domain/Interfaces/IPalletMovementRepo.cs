using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Filters;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPalletMovementRepo
	{		
		void AddPalletMovement(PalletMovement palletMovement);
		Task AddPalletMovementAsync(PalletMovement palletMovement, CancellationToken cancellationToken);
		IQueryable<PalletMovement> GetDataByFilter(PalletMovementSearchFilter filter, string id);		
		Task<bool> CanDeletePalletAsync(string id);
	}
}
