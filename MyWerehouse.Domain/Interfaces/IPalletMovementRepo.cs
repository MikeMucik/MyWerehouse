using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPalletMovementRepo
	{		
		Task AddPalletMovementAsync(PalletMovement palletMovement);
		IQueryable<PalletMovement> GetDataByFilter(PalletMovementSearchFilter filter);		
		Task<bool> CanDeletePalletAsync(string id);		
	}
}
